%define FIRST_BOOTLOADER_SEGMENT_ADDRESS 0x7C0
%define FIRST_BOOTLOADER_STACK_SIZE 4096

%define SECOND_BOOTLOADER_SEGMENT_ADDRESS 0x7E0
%define SECOND_BOOTLOADER_SECTOR_SIZE 1
%define SECOND_BOOTLOADER_STACK_SIZE 4096

	BITS 16

start:
	;
	; 1. Set Data Segment to loaded address
	;
	mov ax, FIRST_BOOTLOADER_SEGMENT_ADDRESS
	mov ds, ax
	mov ss, ax ; set stack segment register
	mov es, ax ; set extra segment register
	
	mov sp,FIRST_BOOTLOADER_STACK_SIZE
	
	;
	; 2. Load 2nd Bootloader
	;	
	mov si, msg_first_bootloader                 ; Print status
	call print_si
	
	mov si, msg_loading_start                    ; Print 'Loading' message
	call print_si	
	mov ax, SECOND_BOOTLOADER_SECTOR_SIZE   
	call print_ax
	mov si, msg_loading_end
	call print_si
	
	; Setup BIOS call to read floppy sector(s)
	mov ax,SECOND_BOOTLOADER_SEGMENT_ADDRESS
	mov es,ax	
	
	mov ax,(0x0200 + SECOND_BOOTLOADER_SECTOR_SIZE)
	mov bx,0
	mov cx,0x0002
	mov dx,0
	
	; Call BIOS to read floppy sector(s)
	int 0x13
	
	; Get Result
	jnc .no_floppy_error
	push ax
	push ax
	
	mov si,label_error
	call print_si
	mov si,label_status
	call print_si
	pop ax
	shr ax,8
	call print_al
	call println
	
	mov si,label_sectors_transferred
	call print_si
	pop ax
	call print_al
	call println
.no_floppy_error:
	
	
	jmp FULL_BOOTLOADER
	

	;
	; String Data
	;
	
	msg_first_bootloader db '1st Bootloader:',0xd,0xa,0
	msg_loading_start db ' Loading 2nd Bootloader (0x',0
	msg_loading_end db ' sectors) from floppy...',0xd,0xa,0
	label_status db 'Status=0x',0
	label_error db 'ERROR: ',0
	label_sectors_transferred db 'SectorsTransferred=0x',0
	msg_boot_complete db 'Boot Complete',0xd,0xa,0	
	msg_done db 'Done',0xd,0xa,0
	
println:
	mov ah,0x0E
	mov al,0xd
	int 0x10
	mov al,0xa
	int 0x10
	ret
	
print_si:                 ; Print string in si
	mov ah, 0x0E          ; Specifies int 10h to print a char'
.print_si_repeat:
	lodsb                 ; al = [si] and si++
	cmp al, 0
	je .print_si_done     ; If char is 0, loop is done
	int 0x10              ; Call BIOS to print the char
	jmp .print_si_repeat  ; repeat
.print_si_done:
	ret  
  
print_ax:
	mov bh,al                 ; Save al	
	shr ax,8                  ; ax >>= 8
	mov ah,0x0E               ; ah = 0x0E (Used to tell BIOS we want to print)
	call print_al_nosetup     ; print al
	mov al,bh                 ; restore al from bh
	call print_al_nosetup     ; print al
	ret
print_al:
	mov ah,0x0E               ; ah = 0x0E (Used to tell BIOS we want to print)
print_al_nosetup:
	mov bl,al                 ; Save al	
	shr al,4                  ; Shift al to print the first nibble
	call .print_al_low_nibble ; print al low nibble	
	mov al,bl                 ; restore al from bl
	and al,0x0F               ; mask lower nibble
	call .print_al_low_nibble ; print al low nibble
	ret	
.print_al_low_nibble:
	cmp al, 9
	jle .print_al_lt9
	add al,('A' - 10 )
	jmp .print_al_print
.print_al_lt9:
	add al,'0'
.print_al_print:
	int 0x10
	ret	
	
	;
	; Pad the rest of the 512 with 0's
	;
	times 510-($-$$) db 0    ; Pad remainder of boot sector with 0s
	dw 0xAA55		         ; The standard PC boot signature
	
	;
	; The 2nd Bootloader
	;	
	msg_second_bootloader db '2nd Bootloader:',0xd,0xa,0
	
FULL_BOOTLOADER:
	;
	; Load new segment registers
	;	
	mov sp,SECOND_BOOTLOADER_STACK_SIZE
	
	mov si, msg_second_bootloader                 ; Print status
	call print_si
	
	
	
	
	
	
	
	
	; set cursor position
	mov ah,2
	mov dx,0x1800
	mov bh,1
	int 0x10
	
	mov ax,0x0501
	int 0x10
	
	mov si, label_status
	call print_si
	
	
	;mov ah,0
	;int 0x16
	
	;mov ax,0x0501
	;int 0x10
	
	
	
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
	
	
	
	db 0x12,0x34
	times 4096-($-$$) db 0	; Pad remainder of boot sector with 0s
	
	
	