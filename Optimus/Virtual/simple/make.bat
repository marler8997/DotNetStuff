
rm simple.bin simple.img

REM Assemble Operating System
nasm -f bin -o simple.bin simple.asm

REM Create Floppy Image
dd bs=512 count=2880 if=simple.bin of=simple.img