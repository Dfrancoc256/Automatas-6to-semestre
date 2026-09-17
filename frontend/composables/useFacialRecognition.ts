import * as faceapi from 'face-api.js'

let modelosPromesa: Promise<void> | null = null

async function cargarModelos() {
  if (!modelosPromesa) {
    const MODEL_URL = '/models'
    modelosPromesa = Promise.all([
      faceapi.nets.tinyFaceDetector.loadFromUri(MODEL_URL),
      faceapi.nets.faceLandmark68Net.loadFromUri(MODEL_URL),
      faceapi.nets.faceRecognitionNet.loadFromUri(MODEL_URL)
    ]).then(() => undefined)
  }
  return modelosPromesa
}

/**
 * Detecta un único rostro en la imagen/video y devuelve su descriptor facial
 * (128 valores). Mismo concepto que el "encoding_facial" de biometrico, pero
 * calculado en el navegador con face-api.js en vez de un servicio Python externo.
 */
async function obtenerDescriptor(
  input: HTMLVideoElement | HTMLCanvasElement
): Promise<number[] | null> {
  await cargarModelos()

  const deteccion = await faceapi
    .detectSingleFace(input, new faceapi.TinyFaceDetectorOptions())
    .withFaceLandmarks()
    .withFaceDescriptor()

  return deteccion ? Array.from(deteccion.descriptor) : null
}

export function useFacialRecognition() {
  return { cargarModelos, obtenerDescriptor }
}
