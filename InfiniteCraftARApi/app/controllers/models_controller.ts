import Model from '#models/model'
import { cuid } from '@adonisjs/core/helpers'
import type { HttpContext } from '@adonisjs/core/http'
import drive from '@adonisjs/drive/services/main'

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
    return response.json({ url })
  }
}
