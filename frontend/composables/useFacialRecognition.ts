import * as faceapi from 'face-api.js'

type Fuente = HTMLVideoElement | HTMLCanvasElement

let modelosPromesa: Promise<void> | null = null
let calentadoPromesa: Promise<void> | null = null

// 224 px es ~3 veces más rápido que el valor por defecto (416) y sobra para un rostro
// a distancia de webcam. El reconocimiento en sí (128-d) recorta y alinea el rostro,
// así que la precisión del descriptor no depende de este tamaño.
const opcionesDetector = () => new faceapi.TinyFaceDetectorOptions({ inputSize: 224, scoreThreshold: 0.5 })

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
 * Carga los modelos y ejecuta una pasada de prueba para que los shaders de WebGL queden
 * compilados: sin esto, el primer escaneo real paga ese costo (~1 s) delante del usuario.
 */
function calentar() {
  if (!calentadoPromesa) {
    calentadoPromesa = (async () => {
      await cargarModelos()
      const lienzo = (lado: number) => {
        const c = document.createElement('canvas')
        c.width = c.height = lado
        return c
      }
      // Una imagen en blanco no contiene rostro, así que la cadena detector → landmarks →
      // descriptor se cortaría en el primer paso. Se ejecuta cada red por separado con su
      // tamaño de entrada real (detector 224, landmarks 112, reconocimiento 150).
      await faceapi.detectSingleFace(lienzo(224), opcionesDetector())
      await faceapi.nets.faceLandmark68Net.detectLandmarks(lienzo(112))
      await faceapi.nets.faceRecognitionNet.computeFaceDescriptor(lienzo(150))
    })().catch(() => { calentadoPromesa = null })
  }
  return calentadoPromesa
}

export interface CajaRostro { x: number, y: number, ancho: number, alto: number, puntaje: number }

/**
 * Detección rápida (solo la caja del rostro, sin landmarks ni descriptor). Sirve para guiar
 * al usuario en tiempo real y decidir cuándo vale la pena calcular el descriptor completo.
 */
async function detectarRostro(input: Fuente): Promise<CajaRostro | null> {
  await cargarModelos()
  const deteccion = await faceapi.detectSingleFace(input, opcionesDetector())
  if (!deteccion) return null
  const { x, y, width, height } = deteccion.box
  return { x, y, ancho: width, alto: height, puntaje: deteccion.score }
}

/**
 * Detecta un único rostro en la imagen/video y devuelve su descriptor facial
 * (128 valores). Mismo concepto que el "encoding_facial" de biometrico, pero
 * calculado en el navegador con face-api.js en vez de un servicio Python externo.
 */
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
