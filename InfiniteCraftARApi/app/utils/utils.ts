import stringSimilarity from 'string-similarity'
import env from '#start/env'

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

  try {
    const response = await fetch(urlInfiniteCraft, { method: 'GET' })

    if (!response.ok) {
      throw new Error(`HTTP error! Status: ${response.status}`)
    }

    const data: any = await response.json()
    return data.result
  } catch (error) {
    console.error('Erreur lors de la requête :', error)
    throw error
  }
}

export async function retrieve3dTask(taskId: string) {
  let taskResults: any
  while (true) {
    try {
      const response = await fetch(`https://api.meshy.ai/openapi/v2/text-to-3d/${taskId}`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${env.get('MESHYAI_API_KEY')}`,
        },
      })
      const data: any = await response.json()

      if (data.progress === 100) {
        taskResults = data
        break
      }
    } catch (error) {
      console.error(error)
      break
    }
  }

  return taskResults
}
