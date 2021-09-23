#include "Main.h"


#include <windows.h>

bool alreadyRan = false;

int main(){}

void Init()
{
	if (alreadyRan)
	{
		alreadyRan = true;
		auto hGameWindow = GetActiveWindow();
		MessageBox(hGameWindow, L"Injection successful!", L"5D Chess Data Interface Bridge", MB_OK);
	}
}

void SetDrawStructPtr()
{

}

/// <summary>
/// Called by injection right before draw-buffers are swapped
/// </summary>
void OnDrawLastHook()
{

}