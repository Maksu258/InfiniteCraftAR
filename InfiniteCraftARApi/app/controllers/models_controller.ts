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
  keyToUse,
  retrieve3dTask,
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
    logger.info('Result', result)
    return response.status(200).send(result.slice(0, 2))
  }

  public async generateFusionWord({ request, response }: HttpContext) {
    const { word1, word2 } = request.only(['word1', 'word2'])

    logger.info('Generating fusion word for ' + word1 + ' and ' + word2)
    if (!word1 || !word2) {
      logger.error('Missing word parameter')
      return response.status(400).send({ error: 'Missing word parameter' })
    }

    const fusionWord = await generateFusionWord(word1, word2)
    logger.info('Fusion word generated : ' + fusionWord)
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

    const headers = { Authorization: `Bearer ${await keyToUse()}` }
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

      console.log('response', response)
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
    let modelUrls = null

    logger.info('Waiting for model to be ready for model: ' + decodedWord)
    try {
      const data = await retrieve3dTask(modelTaskId, headers)
      modelUrls = data.model_urls
    } catch (error) {
      console.error(error)
    }

    if (!modelUrls) {
      logger.error('Create model failed, modelUrls not found for model: ' + decodedWord)
      return response.status(404).send({ error: 'Create model failed, modelUrls not found' })
    }

    const objUrl = modelUrls.obj
    const objResponse = await fetch(objUrl)
    const objBuffer = await objResponse.arrayBuffer()
    const objKey = `${decodedWord}-${cuid()}.obj`

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
      mode: 'refine',
      preview_task_id: modelTaskId,
      enable_pbr: true,
    }

    try {
      const textureResponse = await fetch('https://api.meshy.ai/openapi/v2/text-to-3d', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          ...headers,
        },
        body: JSON.stringify(texturePayload),
      })

      const textureData = (await textureResponse.json()) as { result: string }
      textureId = textureData.result
    } catch (error) {
      logger.error('Error fetching texture for model: ' + decodedWord, error)
      return response.status(500).send({ error: 'Error fetching texture' })
    }

    model = await Model.create({
      name: decodedWord,
      modelUrl: s3ObjUrl,
      mtlUrl: textureId,
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

    const regex = /^\d/

    if (!regex.test(model.mtlUrl)) {
      logger.info('Texture already generated for model: ' + params.id + '(' + model.name + ')')
      return response.status(200).send(model.toJSON())
    }

    const taskId = model.mtlUrl

    logger.info('Waiting for texture to be ready for model: ' + params.id + '(' + model.name + ')')
    let headers = { Authorization: `Bearer ${await keyToUse(model.id)}` }
    const apiResponse = await retrieve3dTask(taskId, headers)
    const mtlUrl = apiResponse.model_urls.mtl
    const pngUrl = apiResponse.texture_urls[0].base_color

    const mtlResponse = await fetch(mtlUrl)
    const mtlBuffer = await mtlResponse.arrayBuffer()
    const mtlKey = `${model.name}-${cuid()}.mtl`

    const pngResponse = await fetch(pngUrl)
    const pngBuffer = await pngResponse.arrayBuffer()
    const pngKey = `${model.name}-${cuid()}.png`

    try {
      logger.info('Uploading mtl and png to S3 for model: ' + params.id + '(' + model.name + ')')
      await drive.use('s3').put(mtlKey, Buffer.from(mtlBuffer), {
        visibility: 'public',
        contentType: 'text/plain',
      })
      await drive.use('s3').put(pngKey, Buffer.from(pngBuffer), {
        visibility: 'public',
        contentType: 'image/png',
      })

      const s3MtlUrl = await drive.use().getUrl(`./${mtlKey}`)
      const s3PngUrl = await drive.use().getUrl(`./${pngKey}`)
      logger.info(
        'MTL and PNG uploaded successfully to S3 for model: ' + params.id + '(' + model.name + ')',
        {
          s3MtlUrl,
          s3PngUrl,
        }
      )
      model.mtlUrl = s3MtlUrl
      model.pngUrl = s3PngUrl
      await model.save()
      return response.status(200).send(model.toJSON())
    } catch (error) {
      logger.error(
        'Error uploading texture to S3 for model: ' + params.id + '(' + model.name + ')',
        error
      )
      return response.status(500).send({ error: 'Error uploading texture to S3' })
    }
  }
}
