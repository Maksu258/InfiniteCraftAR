import Model from '#models/model'
import { cuid } from '@adonisjs/core/helpers'
import type { HttpContext } from '@adonisjs/core/http'
import drive from '@adonisjs/drive/services/main'
import AWS from 'aws-sdk'
import stringSimilarity from 'string-similarity'
import env from '#start/env'
export default class ModelsController {
  public async index(ctx: HttpContext) {
    const models = await Model.all()
    return ctx.response.json(models)
  }

  public async uploadImage({ request, response }: HttpContext) {
    const image = request.file('file', {
      size: '2mb',
      extnames: ['jpeg', 'jpg', 'png'],
    })
    if (!image) {
      return response.badRequest({ error: 'Image missing' })
    }

    const key = `./${cuid()}.${image.extname}`
    await image.moveToDisk(key, 's3', {
      contentType: 'image/png',
    })

    const url = await drive.use().getUrl(key)

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
          Name: 'img.jpg',
        },
      },
      MaxLabels: 10,
      MinConfidence: 60,
    }

    // Appeler Amazon Rekognition
    rekognition.detectLabels(
      params,
      async (err: AWS.AWSError, rekognitionData: AWS.Rekognition.DetectLabelsResponse) => {
        if (err) {
          console.error('Erreur :', err)
          return response.status(500).json({ error: 'Error processing image with Rekognition' })
        } else {
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

          fetchLabels(
            urlClarifai,
            { method: 'POST', headers: headersClarifai, body: bodyClarifai },
            async (clarifaiData: any) => {
              const clarifaiLabels = clarifaiData.outputs[0].data.concepts.map((concept: any) =>
                concept.name.toLowerCase()
              )
              console.log('Labels Clarifai :', clarifaiLabels)

              const urlApi4Ai = 'https://demo.api4ai.cloud/general-det/v1/results'
              const formData = new FormData()
              formData.append(
                'url',
                'https://unityinfinitecraftbucket.s3.us-east-1.amazonaws.com/img.jpg'
              )

              fetchLabels(urlApi4Ai, { method: 'POST', body: formData }, (data: any) => {
                const api4aiLabels: any[] = []
                const results = data.results

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

                displayCommonLabels(commonLabels)

                // Return the found labels in the response
                return response.json({
                  url,
                  rekognitionLabels,
                  clarifaiLabels,
                  api4aiLabels,
                  commonLabels,
                })
              })
            }
          )
        }
      }
    )
  }
}

function fetchLabels(
  url: string | URL | Request,
  options: RequestInit | undefined,
  processLabels: ((value: unknown) => unknown) | null | undefined
) {
  return fetch(url, options)
    .then((response) => {
      if (!response.ok) {
        throw new Error(`HTTP error! Status: ${response.status}`)
      }
      return response.json()
    })
    .then(processLabels)
    .catch((error) => {
      console.error('Erreur lors de la requête :', error)
    })
}

function compareLabels(labels1: any[], labels2: any[], source: any) {
  const similarityThreshold = 0.6
  const commonLabels: { source: any; label1: any; label2: any; similarity: string }[] = []

  labels1.forEach((label1) => {
    labels2.forEach((label2) => {
      const similarity = stringSimilarity.compareTwoStrings(label1, label2)
      if (similarity >= similarityThreshold) {
        commonLabels.push({
          source,
          label1,
          label2,
          similarity: (similarity * 100).toFixed(2),
        })
      }
    })
  })

  return commonLabels
}

function displayCommonLabels(
  commonLabels: { source?: any; label1: any; label2: any; similarity?: any }[]
) {
  if (commonLabels.length > 0) {
    console.log('Éléments communs ou similaires trouvés :')
    console.table(
      commonLabels.map(({ source, label1, label2, similarity }) => ({
        Source: source,
        Label1: label1,
        Label2: label2,
        Similarity: `${similarity}%`,
      }))
    )

    const labelCounts: Record<string, number> = {}
    commonLabels.forEach(({ label1, label2 }) => {
      ;[label1, label2].forEach((label) => {
        if (label) {
          labelCounts[label] = (labelCounts[label] || 0) + 1
        }
      })
    })

    const sortedLabels = Object.entries(labelCounts).sort((a, b) => b[1] - a[1])
    const topTwoLabels = sortedLabels.slice(0, 2)
    console.log('Les 2 labels les plus trouvés :')
    topTwoLabels.forEach(([label, count]) => {
      console.log(`- "${label}" trouvé ${count} fois`)
    })
  } else {
    console.log('Aucune correspondance trouvée.')
  }
}
