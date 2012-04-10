
	
	; Set Video Mode
	mov ax,0x0013
	int 0x10	
	
	
	//
	// draw square
	//
	mov ah,0x0C	
		
	mov al,0011b
	mov bx,310
	mov cx,9
	mov dx,4
	mov si,15
	
	call draw_square	
	
	
draw_square:
	; CONTEXT:
	;   AX = 0x0C
	;   AL = color
	;   BX = column-limit
	;   CX = column-start
	;   DX = row-start
	;   SI = row-limit
	cmp cx,bx              ; Check that column-limit > column-start
	jge .draw_square_end
.draw_square_loop:	
	cmp dx,si
	jge .draw_square_end
	mov di,cx
	call draw_horz_line
	mov cx,di
	inc dx
	jmp .draw_square_loop
.draw_square_end:
	ret
draw_horz_line:
	; CONTEXT:
	;   AX = 0x0C
	;   AL = color
	;   BX = column-limit
	;   CX = column-start
	;   DX = row
	cmp cx,bx
	jge .draw_horz_line_end
	int 0x10
	inc cx
	jmp draw_horz_line
.draw_horz_line_end:
	ret
	