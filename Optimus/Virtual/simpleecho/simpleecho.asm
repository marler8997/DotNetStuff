
	BITS 16

start:
	mov ax, 0x07C0		; Set up 4K stack space after this bootloader
	add ax, 288		; (4096 + 512) / 16 bytes per paragraph
	mov ss, ax
	mov sp, 4096

	mov ax, 0x07C0		; Set data segment to where we're loaded
	mov ds, ax

	
	;
	; Print Boot Complete
	;
	mov si, msg_boot_complete	; Put string position into SI
	call print	; Call our string-printing routine


	;
	; Console Loop
	;
CONSOLE_LOOP:
	;
	; Read character
	;
	mov ah,0x0
	int 0x16
	
	;
	; Echo character
	;
	mov ah,0x0E
	int 0x10
	
	
	jmp CONSOLE_LOOP
	

	;
	; String Data
	;
	msg_boot_complete db 'Boot Complete',0xd,0xa,0	
	
	;
	; Waste Time Function
	;
wait_ax_to_0:
	cmp ax,0
	je .wait_ax_to_0_done
	dec ax	
	jmp wait_ax_to_0
.wait_ax_to_0_done:
	ret
	

print:			; Routine: output string in SI to screen
	mov ah, 0x0E		; int 10h 'print char' function

.repeat:
	lodsb			; Get character from string
	cmp al, 0
	je .done		; If char is zero, end of string
	int 0x10			; Otherwise, print it
	jmp .repeat

.done:
	ret
	
	

	times 510-($-$$) db 0	; Pad remainder of boot sector with 0s
	dw 0xAA55		; The standard PC boot signature