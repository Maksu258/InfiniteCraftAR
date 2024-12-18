import stringSimilarity from 'string-similarity'

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
  const urlInfiniteCraft = `https://neal.fun/api/infinite-craft/pair?first=${word1}&second=${word2}`

  const headers = {
    'accept': '*/*',
    'accept-language': 'fr-FR,fr;q=0.9,en-US;q=0.8,en;q=0.7',
    'dnt': '1',
    'priority': 'u=1, i',
    'referer': 'https://neal.fun/infinite-craft/',
    'sec-ch-ua': '"Not?A_Brand";v="99", "Chromium";v="130"',
    'sec-ch-ua-mobile': '?0',
    'sec-ch-ua-platform': '"macOS"',
    'sec-fetch-dest': 'empty',
    'sec-fetch-mode': 'cors',
    'sec-fetch-site': 'same-origin',
  }

  try {
    const response = await fetch(urlInfiniteCraft, { method: 'POST', headers })
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
