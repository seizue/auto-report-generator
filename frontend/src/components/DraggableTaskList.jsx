import { useState } from 'react'
import {
  DndContext,
  closestCenter,
  KeyboardSensor,
  PointerSensor,
  useSensor,
  useSensors,
} from '@dnd-kit/core'
import {
  arrayMove,
  SortableContext,
  sortableKeyboardCoordinates,
  useSortable,
  verticalListSortingStrategy,
} from '@dnd-kit/sortable'
import { CSS } from '@dnd-kit/utilities'
import { GripVertical, Trash2 } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import styles from './DraggableTaskList.module.css'

function SortableTask({ task, index, listStyle, onUpdate, onRemove, canRemove, statusOptions }) {
  const { t } = useTranslation()
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({ id: task.id })

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
    opacity: isDragging ? 0.5 : 1,
  }

  return (
    <div
      ref={setNodeRef}
      style={style}
      className={`${styles.taskRow} ${isDragging ? styles.dragging : ''}`}
    >
      <button
        className={styles.dragHandle}
        {...attributes}
        {...listeners}
        aria-label="Drag to reorder"
      >
        <GripVertical size={16} />
      </button>
      
      <span className={styles.taskMarker}>
        {listStyle === 'bullets' ? '•' : `${index + 1}.`}
      </span>
      
      <input
        className={styles.taskInput}
        value={task.task}
        onChange={(e) => onUpdate(task.id, 'task', e.target.value)}
        placeholder={`${t('home.taskPlaceholder')} ${index + 1}...`}
      />
      
      <select
        value={task.status}
        onChange={(e) => onUpdate(task.id, 'status', e.target.value)}
        className={styles.statusSelect}
      >
        {statusOptions.map(s => (
          <option key={s} value={s}>{t(`status.${s.toLowerCase().replace(' ', '')}`)}</option>
        ))}
      </select>
      
      {canRemove && (
        <button
          type="button"
          className={styles.removeBtn}
          onClick={() => onRemove(task.id)}
          aria-label="Remove task"
        >
          <Trash2 size={14} />
        </button>
      )}
    </div>
  )
}

export default function DraggableTaskList({ tasks, setTasks, listStyle, statusOptions }) {
  const sensors = useSensors(
    useSensor(PointerSensor, {
      activationConstraint: {
        distance: 8, // 8px movement required before drag starts
      },
    }),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    })
  )

  const handleDragEnd = (event) => {
    const { active, over } = event

    if (active.id !== over.id) {
      setTasks((items) => {
        const oldIndex = items.findIndex((item) => item.id === active.id)
        const newIndex = items.findIndex((item) => item.id === over.id)
        return arrayMove(items, oldIndex, newIndex)
      })
    }
  }

  const updateTask = (id, field, value) => {
    setTasks((items) =>
      items.map((item) => (item.id === id ? { ...item, [field]: value } : item))
    )
  }

  const removeTask = (id) => {
    setTasks((items) => items.filter((item) => item.id !== id))
  }

  return (
    <DndContext
      sensors={sensors}
      collisionDetection={closestCenter}
      onDragEnd={handleDragEnd}
    >
      <SortableContext items={tasks} strategy={verticalListSortingStrategy}>
        <div className={styles.taskList}>
          {tasks.map((task, index) => (
            <SortableTask
              key={task.id}
              task={task}
              index={index}
              listStyle={listStyle}
              onUpdate={updateTask}
              onRemove={removeTask}
              canRemove={tasks.length > 1}
              statusOptions={statusOptions}
            />
          ))}
        </div>
      </SortableContext>
    </DndContext>
  )
}
