import { useRef, useState } from 'react'
import { FileText, Loader2, X, FileImage, File } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import toast from 'react-hot-toast'
import { extractTextFromImage } from '../services/api'
import styles from './UniversalFileUpload.module.css'

/**
 * Universal file upload component supporting PDF, DOCX, and images
 * Extracts text from any supported file type
 */
export default function UniversalFileUpload({ onExtracted }) {
  const { t } = useTranslation()
  const inputRef = useRef(null)
  const [loading, setLoading] = useState(false)
  const [file, setFile] = useState(null)
  const [dragging, setDragging] = useState(false)

  const getFileIcon = (fileName) => {
    const ext = fileName?.split('.').pop()?.toLowerCase()
    if (['jpg', 'jpeg', 'png', 'bmp', 'tiff', 'webp'].includes(ext)) {
      return <FileImage size={20} />
    }
    return <FileText size={20} />
  }

  const process = async (uploadedFile) => {
    if (!uploadedFile) return
    
    const allowed = ['application/pdf', 'application/vnd.openxmlformats-officedocument.wordprocessingml.document', 
                     'image/jpeg', 'image/png', 'image/bmp', 'image/tiff', 'image/webp']
    
    const ext = uploadedFile.name.split('.').pop()?.toLowerCase()
    const isAllowed = allowed.includes(uploadedFile.type) || 
                     ['pdf', 'docx', 'jpg', 'jpeg', 'png', 'bmp', 'tiff', 'webp'].includes(ext)
    
    if (!isAllowed) {
      toast.error('Unsupported file type. Use PDF, DOCX, JPG, PNG, BMP, TIFF, or WEBP')
      return
    }

    setFile(uploadedFile)
    setLoading(true)
    const isPdf = ext === 'pdf'
    const toastId = toast.loading(
      isPdf 
        ? `Extracting text from PDF... (may use OCR if needed)` 
        : `Extracting text from ${ext?.toUpperCase()}...`
    )
    
    try {
      const fd = new FormData()
      fd.append('file', uploadedFile)
      const { data } = await extractTextFromImage(fd)
      
      if (!data.text) {
        toast.error('No text detected in file', { id: toastId })
      } else {
        onExtracted(data.text)
        const lines = data.text.split('\n').length
        toast.success(`Extracted ${lines} line${lines !== 1 ? 's' : ''} from ${uploadedFile.name}`, { id: toastId })
      }
    } catch (error) {
      const errorMsg = error.response?.data?.error || 'Text extraction failed. Is the backend running?'
      toast.error(errorMsg, { id: toastId })
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
    setFile(null)
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
        accept=".pdf,.docx,.jpg,.jpeg,.png,.bmp,.tiff,.webp"
        style={{ display: 'none' }}
        onChange={handleFile}
      />

      {file ? (
        <div className={styles.filePreview}>
          <div className={styles.fileIcon}>
            {getFileIcon(file.name)}
          </div>
          <div className={styles.fileInfo}>
            <div className={styles.fileName}>{file.name}</div>
            <div className={styles.fileSize}>
              {(file.size / 1024).toFixed(1)} KB
            </div>
          </div>
          {!loading && (
            <button className={styles.clearBtn} onClick={clear} title="Remove">
              <X size={16} />
            </button>
          )}
          {loading && (
            <div className={styles.loadingIndicator}>
              <Loader2 size={18} className={styles.spin} />
            </div>
          )}
        </div>
      ) : (
        <div className={styles.placeholder}>
          {loading ? (
            <Loader2 size={18} className={styles.spin} />
          ) : (
            <File size={18} />
          )}
          <span className={styles.mainText}>
            {loading ? 'Extracting text...' : 'Upload document to extract text'}
          </span>
          <span className={styles.hint}>
            PDF, DOCX, JPG, PNG, BMP, TIFF · drag & drop or click
          </span>
        </div>
      )}
    </div>
  )
}
