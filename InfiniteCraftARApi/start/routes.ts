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
router.post('models/analyze-image', [ModelsController, 'analyzeImage'])
router.post('models/generate-fusion-word', [ModelsController, 'generateFusionWord'])
router.get('models/get3d-object/:word', [ModelsController, 'get3DObject'])
router.get('models/get-texture/:id', [ModelsController, 'getTexture'])
