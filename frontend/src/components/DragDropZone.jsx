import { useState, useCallback } from 'react'
import { Upload, Image, FileText, X } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import styles from './DragDropZone.module.css'

export default function DragDropZone({ onFileDrop, accept = 'image/*', multiple = false, children }) {
  const { t } = useTranslation()
  const [isDragging, setIsDragging] = useState(false)
  const [dragCounter, setDragCounter] = useState(0)

  const handleDragEnter = useCallback((e) => {
    e.preventDefault()
    e.stopPropagation()
    setDragCounter((prev) => prev + 1)
    if (e.dataTransfer.items && e.dataTransfer.items.length > 0) {
      setIsDragging(true)
    }
  }, [])

  const handleDragLeave = useCallback((e) => {
    e.preventDefault()
    e.stopPropagation()
    setDragCounter((prev) => {
      const newCounter = prev - 1
      if (newCounter === 0) {
        setIsDragging(false)
      }
      return newCounter
    })
  }, [])

  const handleDragOver = useCallback((e) => {
    e.preventDefault()
    e.stopPropagation()
  }, [])

  const handleDrop = useCallback(
    (e) => {
      e.preventDefault()
      e.stopPropagation()
      setIsDragging(false)
      setDragCounter(0)

      const files = Array.from(e.dataTransfer.files)
      if (files.length > 0) {
        onFileDrop(multiple ? files : files[0])
      }
    },
    [onFileDrop, multiple]
  )

  const handleFileInput = useCallback(
    (e) => {
      const files = Array.from(e.target.files)
      if (files.length > 0) {
        onFileDrop(multiple ? files : files[0])
      }
      e.target.value = '' // Reset input
    },
    [onFileDrop, multiple]
  )

  return (
    <div
      className={`${styles.dropZone} ${isDragging ? styles.dragging : ''}`}
      onDragEnter={handleDragEnter}
      onDragLeave={handleDragLeave}
      onDragOver={handleDragOver}
      onDrop={handleDrop}
    >
      {children || (
        <div className={styles.dropContent}>
          <div className={styles.icon}>
            <Upload size={32} />
          </div>
          <p className={styles.text}>
            Drag & drop {accept.includes('image') ? 'images' : 'files'} here
          </p>
          <p className={styles.subtext}>or click to browse</p>
          <input
            type="file"
            accept={accept}
            multiple={multiple}
            onChange={handleFileInput}
            className={styles.fileInput}
          />
        </div>
      )}
      {isDragging && (
        <div className={styles.overlay}>
          <div className={styles.overlayContent}>
            <Upload size={48} />
            <p>Drop {accept.includes('image') ? 'images' : 'files'} here</p>
          </div>
        </div>
      )}
    </div>
  )
}

export function FilePreview({ file, onRemove }) {
  const isImage = file.type?.startsWith('image/')

  return (
    <div className={styles.filePreview}>
      <div className={styles.fileIcon}>
        {isImage ? <Image size={20} /> : <FileText size={20} />}
      </div>
      <div className={styles.fileInfo}>
        <div className={styles.fileName}>{file.name}</div>
        <div className={styles.fileSize}>
          {(file.size / 1024).toFixed(1)} KB
        </div>
      </div>
      {onRemove && (
        <button
          className={styles.removeFile}
          onClick={onRemove}
          aria-label="Remove file"
        >
          <X size={16} />
        </button>
      )}
    </div>
  )
}
