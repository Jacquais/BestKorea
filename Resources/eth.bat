@echo off
set coin=%0

if "%coin%"=="xmr" (
    start "" NsCpuCNMiner64.exe -o stratum+ssl://xmr-eu1.nanopool.org:14433 -u YOUR_XMR_ADDRESS.0.PLACEHOLDER/lol@gmail.com -p z
) else (
	setx GPU_USE_SYNC_OBJECTS 1
	setx GPU_MAX_HEAP_SIZE 100
	start "" EthDcrMiner64.exe -epool eth-us-east1.nanopool.org:9999 -ewal 0x2a156c6dd3bdf2a0c5b284b45b2396c053c2a63d.Jacquais1/sullyjack1717@gmail.com -epsw x -mode 1 -ftime 10
)
