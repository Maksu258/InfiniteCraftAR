import Model from '#models/model'
import { cuid } from '@adonisjs/core/helpers'
import type { HttpContext } from '@adonisjs/core/http'
import drive from '@adonisjs/drive/services/main'
import AWS from 'aws-sdk'
import env from '#start/env'
import {
  compareLabels,
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
    const image = request.file('file', {
      size: '2mb',
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

    const commonLabels = [
      ...compareLabels(rekognitionLabels, clarifaiLabels, 'Rekognition vs Clarifai'),
      ...compareLabels(rekognitionLabels, api4aiLabels, 'Rekognition vs API4AI'),
      ...compareLabels(clarifaiLabels, api4aiLabels, 'Clarifai vs API4AI'),
    ]

    const result = getCommonLabelsSummary(commonLabels)

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

    const model = await Model.findBy('name', decodeURIComponent(params.word))
    if (model) {
      return response.status(200).send(model)
    }
    return response.status(404).send({ error: 'Model not found' })
  }
}
