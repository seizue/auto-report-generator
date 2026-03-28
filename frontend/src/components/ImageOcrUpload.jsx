import { useRef, useState } from 'react'
import { ImagePlus, Loader2, X } from 'lucide-react'
import toast from 'react-hot-toast'
import { extractTextFromImage } from '../services/api'
import styles from './ImageOcrUpload.module.css'

/**
 * Drag-and-drop / click-to-upload image OCR button.
 * onExtracted(text) is called with the extracted text.
 */
export default function ImageOcrUpload({ onExtracted }) {
  const inputRef = useRef(null)
  const [loading, setLoading] = useState(false)
  const [preview, setPreview] = useState(null)
  const [dragging, setDragging] = useState(false)

  const process = async (file) => {
    if (!file) return
    const allowed = ['image/jpeg', 'image/png', 'image/bmp', 'image/tiff', 'image/webp']
    if (!allowed.includes(file.type)) {
      toast.error('Use JPG, PNG, BMP, TIFF, or WEBP')
      return
    }
    setPreview(URL.createObjectURL(file))
    setLoading(true)
    const toastId = toast.loading('Extracting text from image...')
    try {
      const fd = new FormData()
      fd.append('file', file)
      const { data } = await extractTextFromImage(fd)
      if (!data.text) {
        toast.error('No text detected in image', { id: toastId })
      } else {
        onExtracted(data.text)
        toast.success(`Extracted ${data.text.split('\n').length} line(s)`, { id: toastId })
      }
    } catch {
      toast.error('OCR failed. Is the backend running?', { id: toastId })
    } finally {
      setLoading(false)
    }
  }

  const handleFile = (e) => process(e.target.files?.[0])

  const handleDrop = (e) => {
    e.preventDefault()
    setDragging(false)
    process(e.dataTransfer.files?.[0])
  }

  const clear = (e) => {
    e.stopPropagation()
    setPreview(null)
    if (inputRef.current) inputRef.current.value = ''
  }

  return (
    <div
      className={`${styles.zone} ${dragging ? styles.dragging : ''}`}
      onClick={() => !loading && inputRef.current?.click()}
      onDragOver={(e) => { e.preventDefault(); setDragging(true) }}
      onDragLeave={() => setDragging(false)}
      onDrop={handleDrop}
    >
      <input
        ref={inputRef}
        type="file"
        accept="image/jpeg,image/png,image/bmp,image/tiff,image/webp"
        style={{ display: 'none' }}
        onChange={handleFile}
      />

      {preview ? (
        <div className={styles.previewWrap}>
          <img src={preview} alt="preview" className={styles.previewImg} />
          {!loading && (
            <button className={styles.clearBtn} onClick={clear} title="Remove">
              <X size={12} />
            </button>
          )}
          {loading && (
            <div className={styles.overlay}>
              <Loader2 size={22} className={styles.spin} />
              <span>Extracting...</span>
            </div>
          )}
        </div>
      ) : (
        <div className={styles.placeholder}>
          {loading
            ? <Loader2 size={18} className={styles.spin} />
            : <ImagePlus size={18} />}
          <span>{loading ? 'Extracting text...' : 'Upload image to extract text'}</span>
          <span className={styles.hint}>JPG, PNG, BMP, TIFF · drag & drop or click</span>
        </div>
      )}
    </div>
  )
}
