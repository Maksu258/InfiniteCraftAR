/*
|--------------------------------------------------------------------------
| Routes file
|--------------------------------------------------------------------------
|
| The routes file is used for defining the HTTP routes.
|
*/

import router from '@adonisjs/core/services/router'
const ModelsController = () => import('#controllers/models_controller')

router.get('models', [ModelsController, 'index'])
router.post('models/upload-image', [ModelsController, 'uploadImage'])
