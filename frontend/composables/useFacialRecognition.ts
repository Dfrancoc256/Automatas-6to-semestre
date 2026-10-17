import * as faceapi from 'face-api.js'

type Fuente = HTMLVideoElement | HTMLCanvasElement | HTMLImageElement

let modelosPromesa: Promise<void> | null = null
let calentadoPromesa: Promise<void> | null = null

// Tamaño de entrada para detección y recorte.
const opcionesDetector = () => new faceapi.TinyFaceDetectorOptions({ inputSize: 320, scoreThreshold: 0.45 })

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

/** Prepara los modelos faciales. */
function calentar() {
  if (!calentadoPromesa) {
    calentadoPromesa = (async () => {
      await cargarModelos()
      const lienzo = (lado: number) => {
        const c = document.createElement('canvas')
        c.width = c.height = lado
        return c
      }
      // Carga cada red con su tamaño de entrada.
      await faceapi.detectSingleFace(lienzo(320), opcionesDetector())
      await faceapi.nets.faceLandmark68Net.detectLandmarks(lienzo(112))
      await faceapi.nets.faceRecognitionNet.computeFaceDescriptor(lienzo(150))
    })().catch(() => { calentadoPromesa = null })
  }
  return calentadoPromesa
}

export interface CajaRostro { x: number, y: number, ancho: number, alto: number, puntaje: number }

/** Detecta la posición de un rostro. */
async function detectarRostro(input: Fuente): Promise<CajaRostro | null> {
  await cargarModelos()
  const deteccion = await faceapi.detectSingleFace(input, opcionesDetector())
  if (!deteccion) return null
  const { x, y, width, height } = deteccion.box
  return { x, y, ancho: width, alto: height, puntaje: deteccion.score }
}

/** Obtiene el descriptor facial. */
async function obtenerDescriptor(input: Fuente): Promise<number[] | null> {
  await cargarModelos()

  const deteccion = await faceapi
    .detectSingleFace(input, opcionesDetector())
    .withFaceLandmarks()
    .withFaceDescriptor()

  return deteccion ? Array.from(deteccion.descriptor) : null
}

export function useFacialRecognition() {
  return { cargarModelos, calentar, detectarRostro, obtenerDescriptor }
}
