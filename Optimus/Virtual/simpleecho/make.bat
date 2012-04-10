
rm simpleecho.bin simpleecho.img

REM Assemble Operating System
nasm -f bin -o simpleecho.bin simpleecho.asm

REM Create Floppy Image
dd bs=512 count=2880 if=simpleecho.bin of=simpleecho.img