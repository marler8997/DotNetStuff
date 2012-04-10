
rm bootstrap.bin bootstrap.img

REM Assemble Operating System
nasm -f bin -o bootstrap.bin bootstrap.asm

REM Create Floppy Image
dd bs=1024 if=bootstrap.bin of=bootstrap.img