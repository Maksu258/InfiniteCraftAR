import stringSimilarity from 'string-similarity'
import env from '#start/env'
import Model from '#models/model'
import logger from '@adonisjs/core/services/logger'

export async function fetchLabels(url: string | URL | Request, options: RequestInit | undefined) {
  const response = await fetch(url, options)
  if (!response.ok) {
    throw new Error(`HTTP error! Status: ${response.status}`)
  }
  return response.json()
}

export function compareLabels(labels1: any[], labels2: any[], source: any) {
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

export function countLabelOccurrences(
  rekognitionLabels: any[],
  clarifaiLabels: any[],
  foundLabels: any[]
) {
  const allLabels = [...rekognitionLabels, ...clarifaiLabels, ...foundLabels]
  const groupedLabels: Record<string, number> = {}

  // Trouver ou créer un groupe pour un label
  function findOrCreateGroup(label: string) {
    let bestMatch = null
    let highestScore = 0

    // Cherche le groupe avec la meilleure similarité
    for (const group in groupedLabels) {
      const similarity = stringSimilarity.compareTwoStrings(group, label)
      if (similarity > 0.66 && similarity > highestScore) {
        highestScore = similarity
        bestMatch = group
      }
    }

    return bestMatch
  }

  // Vérifie si un label est contenu dans un groupe
  function findContainedGroup(label: string) {
    for (const group in groupedLabels) {
      if (group.includes(label) || label.includes(group)) {
        return group
      }
    }
    return null
  }

  allLabels.forEach((label) => {
    const group = findOrCreateGroup(label) || findContainedGroup(label)
    if (group) {
      groupedLabels[group] += 1 // Incrémenter le compteur du groupe trouvé
    } else {
      groupedLabels[label] = 1 // Créer un nouveau groupe avec un compteur
    }
  })

  const sortedLabels = Object.keys(groupedLabels).sort(
    (a, b) => groupedLabels[b] - groupedLabels[a]
  )

  return sortedLabels
}

export function getCommonLabelsSummary(
  commonLabels: { source?: any; label1: any; label2: any; similarity?: any }[]
) {
  const labelCounts: Record<string, number> = {}
  commonLabels.forEach(({ label1, label2 }) => {
    ;[label1, label2].forEach((label) => {
      if (label) {
        labelCounts[label] = (labelCounts[label] || 0) + 1
      }
    })
  })

  const sortedLabels = Object.keys(labelCounts).sort((a, b) => labelCounts[b] - labelCounts[a])
  return sortedLabels
}

export async function generateFusionWord(word1: string, word2: string) {
  const urlInfiniteCraft = `https://infiniteback.org/pair?first=${word1}&second=${word2}`

  logger.info('Url infinite craft: ' + urlInfiniteCraft)
  const response = await fetch(urlInfiniteCraft, { method: 'GET' })

  if (!response.ok) {
    logger.error('Error generating fusion word', response)
    return { result: 'Nothing', emoji: '' }
  }
  if (response == null) {
    logger.info('No fusion for ' + word1 + ' and ' + word2)
    return { result: 'Nothing', emoji: '' }
  }

  const data: any = await response.json()
  logger.info('Data infinite craft: ' + data)
  return data.result
}

export async function retrieve3dTask(taskId: string, headers: any) {
  let taskResults: any
  let progress = 0
  do {
    try {
      const response = await fetch(`https://api.meshy.ai/openapi/v2/text-to-3d/${taskId}`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
          ...headers,
        },
      })
      const data: any = await response.json()
      if (data.progress === 100) {
        taskResults = data
        break
      } else if (data.status === 'FAILED') {
        throw new Error('Task failed')
      }

      progress = data.progress
      logger.info('Task progress : ' + progress + '%')
    } catch (error) {
      logger.error('Error retrieving 3d task', error)
      break
    }
    await new Promise((resolve) => setTimeout(resolve, 10000))
  } while (progress < 100)

  return taskResults
}

export async function keyToUse(id?: number) {
  const keys = [
    env.get('MESHYAI_API_KEY'),
    env.get('MESHYAI_API_KEY2'),
    env.get('MESHYAI_API_KEY3'),
    env.get('MESHYAI_API_KEY4'),
    env.get('MESHYAI_API_KEY5'),
  ]
  if (id) {
    return keys[id % 5]
  } else {
    const lastModel = await Model.query().orderBy('id', 'desc').first()
    const lastId = lastModel ? lastModel.id + 1 : 1
    return keys[lastId % 5]
  }
}
