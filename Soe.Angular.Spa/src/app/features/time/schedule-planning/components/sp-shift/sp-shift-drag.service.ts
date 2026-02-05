import { Injectable } from '@angular/core';

enum DragCursorClass {
  None = '',
  Copy = 'cdk-cursor-copy',
  Move = 'cdk-cursor-alias',
  Grabbing = 'cdk-cursor-grabbing',
}

enum DragKey {
  Control = 'Control',
  Shift = 'Shift',
}

enum KeyEventType {
  KeyDown = 'keydown',
  KeyUp = 'keyup',
}

@Injectable({
  providedIn: 'root',
})
export class SpShiftDragService {
  private isDragging = false;
  private ctrlPressed = false;
  private shiftPressed = false;
  private dragCursorClass: DragCursorClass = DragCursorClass.None;

  onDragStarted() {
    this.isDragging = true;
    this.addKeyListeners();
    this.updateCursor();
  }

  onDragEnded() {
    this.isDragging = false;
    this.removeKeyListeners();
    this.resetCursor();
  }

  private addKeyListeners() {
    window.addEventListener(KeyEventType.KeyDown, this.keydownHandler);
    window.addEventListener(KeyEventType.KeyUp, this.keyupHandler);
  }

  private removeKeyListeners() {
    window.removeEventListener(KeyEventType.KeyDown, this.keydownHandler);
    window.removeEventListener(KeyEventType.KeyUp, this.keyupHandler);
  }

  private keydownHandler = (event: KeyboardEvent) => {
    if (event.key === DragKey.Control) {
      if (!this.ctrlPressed) {
        this.ctrlPressed = true;
        this.updateCursor();
      }
    } else if (event.key === DragKey.Shift) {
      if (!this.shiftPressed) {
        this.shiftPressed = true;
        this.updateCursor();
      }
    }
  };

  private keyupHandler = (event: KeyboardEvent) => {
    if (event.key === DragKey.Control) {
      if (this.ctrlPressed) {
        this.ctrlPressed = false;
        this.updateCursor();
      }
    } else if (event.key === DragKey.Shift) {
      if (this.shiftPressed) {
        this.shiftPressed = false;
        this.updateCursor();
      }
    }
  };

  private updateCursor() {
    // This will change the cursor class based on if Ctrl or Shift is pressed while dragging
    let cursorClass: DragCursorClass = DragCursorClass.None;
    if (this.ctrlPressed) {
      cursorClass = DragCursorClass.Copy;
    } else if (this.shiftPressed) {
      cursorClass = DragCursorClass.Move;
    } else if (this.isDragging) {
      cursorClass = DragCursorClass.Grabbing;
    }

    if (cursorClass !== this.dragCursorClass) {
      this.resetCursor();
      if (cursorClass) {
        document.body.classList.add(cursorClass);
      }
      this.dragCursorClass = cursorClass;
    }
  }

  private resetCursor() {
    document.body.classList.remove(
      DragCursorClass.Copy,
      DragCursorClass.Move,
      DragCursorClass.Grabbing
    );
  }
}
