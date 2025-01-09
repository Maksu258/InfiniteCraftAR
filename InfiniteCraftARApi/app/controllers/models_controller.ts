import Model from '#models/model'
import { cuid } from '@adonisjs/core/helpers'
import type { HttpContext } from '@adonisjs/core/http'
import drive from '@adonisjs/drive/services/main'
import AWS from 'aws-sdk'
import env from '#start/env'
import logger from '@adonisjs/core/services/logger'

import {
  compareLabels,
  countLabelOccurrences,
  fetchLabels,
  generateFusionWord,
  getCommonLabelsSummary,
} from '../utils/utils.js'

export default class ModelsController {
  public async index(ctx: HttpContext) {
    const models = await Model.all()
    return ctx.response.json(models)
  }

  public async analyzeImage({ request, response }: HttpContext) {
    console.log('Analyze image', request)
    const image = request.file('file', {
      size: '20mb',
      extnames: ['jpeg', 'jpg', 'png'],
    })
    if (!image) {
      return response.badRequest({ error: 'Image missing' })
    }

    const key = `${cuid()}.${image.extname}`
    await image.moveToDisk(`./${key}`, 's3', {
      contentType: 'image/png',
    })

    const url = await drive.use().getUrl(`./${key}`)

    AWS.config.update({ region: 'us-east-1' })

    const rekognition = new AWS.Rekognition({
      accessKeyId: env.get('AWS_ACCESS_KEY_ID'),
      secretAccessKey: env.get('AWS_SECRET_ACCESS_KEY'),
      region: 'us-east-1',
    })

    const params = {
      Image: {
        S3Object: {
          Bucket: 'unityinfinitecraftbucket',
          Name: key,
        },
      },
      MaxLabels: 10,
      MinConfidence: 60,
    }

    // Appeler Amazon Rekognition
    const rekognitionData = await new Promise<AWS.Rekognition.DetectLabelsResponse>(
      (resolve, reject) => {
        rekognition.detectLabels(params, (err, data) => {
          if (err) {
            reject(err)
          } else {
            resolve(data)
          }
        })
      }
    ).catch((err) => {
      console.error('Erreur :', err)
      return response.status(500).json({ error: 'Error processing image with Rekognition' })
    })

    if (!rekognitionData) return

    const rekognitionLabels = (rekognitionData.Labels || []).map((label) =>
      label.Name ? label.Name.toLowerCase() : ''
    )
    console.log('Labels Rekognition :', rekognitionLabels)

    const imageUrl = url

    const urlClarifai =
      'https://api.clarifai.com/v2/users/clarifai/apps/main/models/general-image-recognition/versions/aa7f35c01e0642fda5cf400f543e7c40/outputs'
    const headersClarifai = {
      'Authorization': 'Key 484a24b9d6f44ed087a1d6b52c51dbb8',
      'Content-Type': 'application/json',
    }
    const bodyClarifai = JSON.stringify({
      inputs: [
        {
          data: {
            image: {
              url: imageUrl,
            },
          },
        },
      ],
    })

    const clarifaiResponse: any = await fetchLabels(urlClarifai, {
      method: 'POST',
      headers: headersClarifai,
      body: bodyClarifai,
    })
    const clarifaiLabels = clarifaiResponse.outputs[0].data.concepts.map((concept: any) =>
      concept.name.toLowerCase()
    )
    console.log('Labels Clarifai :', clarifaiLabels)

    const urlApi4Ai = 'https://demo.api4ai.cloud/general-det/v1/results'
    const formData = new FormData()
    formData.append('url', imageUrl)

    const api4aiResponse: any = await fetchLabels(urlApi4Ai, {
      method: 'POST',
      body: formData,
    })
    const api4aiLabels: any[] = []
    const results = api4aiResponse.results

    results.forEach((result: any) => {
      const objects = result.entities[0]?.objects || []
      objects.forEach((obj: any) => {
        const entities = obj.entities || []
        entities.forEach((entity: { kind: string; classes: {} }) => {
          if (entity.kind === 'classes' && entity.classes) {
            api4aiLabels.push(...Object.keys(entity.classes))
          }
        })
      })
    })

    console.log('Labels trouvés :', api4aiLabels)

    // const commonLabels = [
    //   ...compareLabels(rekognitionLabels, clarifaiLabels, 'Rekognition vs Clarifai'),
    //   ...compareLabels(rekognitionLabels, api4aiLabels, 'Rekognition vs API4AI'),
    //   ...compareLabels(clarifaiLabels, api4aiLabels, 'Clarifai vs API4AI'),
    // ]

    // const result = getCommonLabelsSummary(commonLabels)
    const result = countLabelOccurrences(rekognitionLabels, clarifaiLabels, api4aiLabels)
    return response.status(200).send(result.slice(0, 2))
  }

  public async generateFusionWord({ request, response }: HttpContext) {
    const { word1, word2 } = request.only(['word1', 'word2'])

    if (!word1 || !word2) {
      return response.status(400).send({ error: 'Missing word parameter' })
    }

    const fusionWord = await generateFusionWord(word1, word2)
    return response.status(200).send(fusionWord)
  }

  public async get3DObject({ params, response }: HttpContext) {
    if (!params.word) {
      return response.status(400).send({ error: 'Missing word parameter' })
    }

    const decodedWord = decodeURIComponent(params.word)
    logger.info('Getting 3D object for model: ' + decodedWord)
    let model = await Model.findBy('name', decodedWord)
    if (model) {
      logger.info('3D object found for model: ' + decodedWord)
      return response.status(200).send(model)
    }

    logger.info('Creating 3D object for model: ' + decodedWord)

    let headers = { Authorization: `Bearer ${env.get('MESHYAI_API_KEY')}` }
    const payload = {
      mode: 'preview',
      prompt: `a ${decodedWord}`,
      art_style: 'realistic',
      should_remesh: true,
    }

    let modelTaskId = null

    try {
      const response = await fetch('https://api.meshy.ai/openapi/v2/text-to-3d', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          ...headers,
        },
        body: JSON.stringify(payload),
      })
      const data = (await response.json()) as { result: string }
      modelTaskId = data.result
    } catch (error) {
      console.error(error)
    }

    if (!modelTaskId) {
      logger.error('Create model failed, modelTaskId not found for model: ' + decodedWord)
      return response.status(404).send({ error: 'Create model failed, modelTaskId not found' })
    }

    console.log('modelTaskId', modelTaskId)
    headers = { Authorization: `Bearer ${env.get('MESHYAI_API_KEY')}` }
    let modelUrls = null

    logger.info('Waiting for model to be ready for model: ' + decodedWord)
    while (true) {
      try {
        const response = await fetch(`https://api.meshy.ai/openapi/v2/text-to-3d/${modelTaskId}`, {
          method: 'GET',
          headers: {
            'Content-Type': 'application/json',
            ...headers,
          },
        })
        const data: any = await response.json()

        if (data.progress === 100) {
          modelUrls = data.model_urls
          break
        }
      } catch (error) {
        console.error(error)
        break
      }
    }

    if (!modelUrls) {
      logger.error('Create model failed, modelUrls not found for model: ' + decodedWord)
      return response.status(404).send({ error: 'Create model failed, modelUrls not found' })
    }

    const objUrl = modelUrls.obj
    const objResponse = await fetch(objUrl)
    const objBuffer = await objResponse.arrayBuffer()
    const objKey = `${cuid()}.obj`

    try {
      await drive.use('s3').put(objKey, Buffer.from(objBuffer), {
        visibility: 'public',
        contentType: 'application/octet-stream',
      })

      logger.info('File uploaded successfully to S3 for model: ' + decodedWord, objKey)
    } catch (error) {
      logger.error('Error uploading file to S3 for model: ' + decodedWord, error)
      return response.status(500).send({ error: 'Error uploading file to S3' })
    }

    const s3ObjUrl = await drive.use().getUrl(`./${objKey}`)

    logger.info('Creating texture for model: ' + decodedWord)
    let textureId = null
    const texturePayload = {
      model_url: modelUrls.obj,
      object_prompt: decodedWord,
      style_prompt: 'a cartoon like texture',
      art_style: 'cartoon-line-art',
    }

    try {
      const textureResponse = await fetch('https://api.meshy.ai/openapi/v1/text-to-texture', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${env.get('MESHYAI_API_KEY')}`,
        },
        body: JSON.stringify(texturePayload),
      })

      const textureData = (await textureResponse.json()) as { result: string }
      textureId = textureData.result
      console.log('textureId', textureId)
      console.log('textureData', textureData)
    } catch (error) {
      logger.error('Error fetching texture for model: ' + decodedWord, error)
      return response.status(500).send({ error: 'Error fetching texture' })
    }

    model = await Model.create({
      name: decodedWord,
      modelUrl: s3ObjUrl,
      textureUrl: textureId,
    })

    logger.info('Model created successfully for model: ' + decodedWord, model.toJSON())
    return response.status(200).send(model.toJSON())
  }

  public async getTexture({ params, response }: HttpContext) {
    logger.info('Getting texture for model: ' + params.id)
    if (!params.id) {
      logger.error('Missing id parameter')
      return response.status(400).send({ error: 'Missing id parameter' })
    }
    const model = await Model.findBy('id', params.id)
    if (!model) {
      logger.error('Model not found')
      return response.status(404).send({ error: 'Model not found' })
    }

    const textureUrl = model.textureUrl
    const regex = /^\d/

    if (!regex.test(textureUrl)) {
      logger.info('Texture already generated for model: ' + params.id)
      return response.status(200).send({ textureUrl })
    }

    const taskId = textureUrl
    const headers = { Authorization: `Bearer ${env.get('MESHYAI_API_KEY')}` }
    let textureData = null

    logger.info('Waiting for texture to be ready for model: ' + params.id)
    while (true) {
      try {
        const apiResponse = await fetch(
          `https://api.meshy.ai/openapi/v1/text-to-texture/${taskId}`,
          {
            method: 'GET',
            headers: headers,
          }
        )
        textureData = (await apiResponse.json()) as { progress: number; texture_urls: any[] }

        if (textureData.progress === 100) {
          logger.info('Texture ready for model: ' + params.id)
          break
        }
      } catch (error) {
        logger.error('Error fetching texture data for model: ' + params.id, error)
        return response.status(500).send({ error: 'Error fetching texture data' })
      }
    }

    console.log('textureData', textureData)
    const normalTextureUrl = textureData.texture_urls[0].normal
    const textureResponse = await fetch(normalTextureUrl)
    const textureBuffer = await textureResponse.arrayBuffer()
    const textureKey = `${cuid()}.png`

    try {
      logger.info('Uploading texture to S3 for model: ' + params.id)
      await drive.use('s3').put(textureKey, Buffer.from(textureBuffer), {
        visibility: 'public',
        contentType: 'image/png',
      })

      const s3TextureUrl = await drive.use().getUrl(`./${textureKey}`)
      logger.info('Texture uploaded successfully to S3 for model: ' + params.id, s3TextureUrl)
      model.textureUrl = s3TextureUrl
      await model.save()
      return response.status(200).send({ textureUrl: s3TextureUrl })
    } catch (error) {
      logger.error('Error uploading texture to S3 for model: ' + params.id, error)
      return response.status(500).send({ error: 'Error uploading texture to S3' })
    }
  }
}
