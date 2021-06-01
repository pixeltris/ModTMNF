// Taken 2021/05/21 https://github.com/pixeltris/SonyAlphaUSB/blob/fd69cbb818f3ce0b0ccc01a734c822ecbe9d1147/WIA%20Logger/SonyAlphaUSBLoader.cpp
#include <windows.h>
#include "min_minhook.h"
#include <stdio.h>
#include <metahost.h>

#pragma comment(lib, "mscoree.lib")
#pragma comment(lib, "user32.lib")

#ifdef __cplusplus
#define LIBRARY_API extern "C" __declspec (dllexport)
#else
#define LIBRARY_API __declspec (dllexport)
#endif

BOOL hooksInitialized = FALSE;

LIBRARY_API MH_STATUS WL_InitHooks()
{
	if (!hooksInitialized)
	{
		hooksInitialized = TRUE;
		return MH_Initialize();
	}
	return MH_OK;
}

LIBRARY_API MH_STATUS WL_HookFunction(LPVOID target, LPVOID detour, LPVOID* original)
{
	MH_STATUS status = MH_CreateHook(target, detour, original);
	if (status == MH_OK)
	{
		return MH_EnableHook(target);
	}
	return status;
}

LIBRARY_API MH_STATUS WL_CreateHook(LPVOID target, LPVOID detour, LPVOID* original)
{
	return MH_CreateHook(target, detour, original);
}

LIBRARY_API MH_STATUS WL_RemoveHook(LPVOID target)
{
	return MH_RemoveHook(target);
}

LIBRARY_API MH_STATUS WL_EnableHook(LPVOID target)
{
	return MH_EnableHook(target);
}

LIBRARY_API MH_STATUS WL_DisableHook(LPVOID target)
{
	return MH_DisableHook(target);
}

typedef unsigned int (*GetMwClassIdFunc_t)(void* ptr);
LIBRARY_API unsigned int GetMwClassId(void* ptr)
{
    return ((GetMwClassIdFunc_t*)*(int*)ptr)[3](ptr);
}

void LoadDotNet()
{
    ICLRMetaHost* metaHost;
    ICLRRuntimeHost* runtimeHost;
    ICLRRuntimeInfo *runtimeInfo;
    HRESULT result = CLRCreateInstance(CLSID_CLRMetaHost, IID_ICLRMetaHost, (LPVOID*)&metaHost);
    if (!SUCCEEDED(result))
    {
        return;
    }
    
    result = metaHost->GetRuntime(L"v4.0.30319", IID_ICLRRuntimeInfo, (LPVOID*)&runtimeInfo);
    if (!SUCCEEDED(result))
    {
        return;
    }
    
    result = runtimeInfo->GetInterface(CLSID_CLRRuntimeHost, IID_ICLRRuntimeHost, (LPVOID*)&runtimeHost);
    if (!SUCCEEDED(result))
    {
        return;
    }
    
    result = runtimeHost->Start();
    if (!SUCCEEDED(result))
    {
        return;
    }
    
    result = runtimeInfo->BindAsLegacyV2Runtime();
    if (!SUCCEEDED(result))
    {
        return;
    }
    
    result = runtimeHost->ExecuteInDefaultAppDomain(L"ModTMNF\\ModTMNF.exe", L"ModTMNF.Program", L"DllMain", NULL, NULL);
}

BOOL FileExists(LPCTSTR szPath)
{
    DWORD dwAttrib = GetFileAttributes(szPath);
    return (dwAttrib != INVALID_FILE_ATTRIBUTES && !(dwAttrib & FILE_ATTRIBUTE_DIRECTORY));
}

typedef int (__stdcall *WinMain_t)(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nShowCmd);
WinMain_t WinMainOriginal;

int WinMainHook(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nShowCmd)
{
    LoadDotNet();
    return WinMainOriginal(hInstance, hPrevInstance, lpCmdLine, nShowCmd);
}

BOOL TryHookWinMain()
{
    WL_InitHooks();
    const char* winmainFile = "ModTMNF\\WinMain.txt";
    if (!FileExists(winmainFile))
    {
        MessageBox(NULL, "Couldn't find WinMain.txt", "Error", 0);
        return false;
    }
    
    FILE* fp = fopen(winmainFile, "r");
    if (fp == NULL)
    {
        MessageBox(NULL, "Failed to open WinMain.txt", "Error", 0);
        return false;
    }
    DWORD addr = 0;
    if (fscanf(fp, "%x", &addr) == 0)
    {
        addr = 0;
    }
    fclose(fp);
    if (!addr)
    {
        MessageBox(NULL, "Failed to read WinMain address from WinMain.txt", "Error", 0);
        return false;
    }
    WL_HookFunction((LPVOID)addr, WinMainHook, (LPVOID*)&WinMainOriginal);
    return true;
}

BOOL WINAPI DllMain(HINSTANCE hDll, DWORD dwReason, LPVOID lpReserved)
{
    switch (dwReason)
    {
        case DLL_PROCESS_ATTACH:
            if (!TryHookWinMain())
            {
                ExitProcess(0);
            }
            DisableThreadLibraryCalls(hDll);
            //CreateThread(NULL, NULL, (LPTHREAD_START_ROUTINE)LoadDotNet, NULL, NULL, NULL);
            break;
    }
    return TRUE;
}