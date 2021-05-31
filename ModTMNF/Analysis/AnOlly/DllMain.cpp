#include <windows.h>
#include <stdio.h>
#include <string>
#include <fstream>
#include <sstream>
#include <iostream>
#include "Plugin.h"

int ODBG_Plugindata(char shortname[32])
{
	strcpy_s(shortname, 32, "Analysis loader");
	return PLUGIN_VERSION;
}

int ODBG_Plugininit(int ollydbgversion, HWND hw, ulong *features)
{
	if (ollydbgversion < PLUGIN_VERSION)
		return -1;

	return 0;
}

int ODBG_Pluginmenu(int origin, char data[4096], void *item)
{
	if (origin != PM_MAIN)
		return 0;

	strcpy_s(data, 4096, "0 &Load");

	return 0;
}

std::string readLine(FILE* file)
{
	char buff[512];
	ZeroMemory(buff, 512);
	fgets(buff, 512, file);

	std::string str(buff);
	int endpos = static_cast<int>(str.find_last_not_of("\n"));
	if (endpos != std::string::npos)
		str = str.substr(0, endpos + 1);
	return str;
}

void handleContent(std::string content, bool isLabel)
{
	std::string addrStr = content.substr(0, 8);
	std::string addrLabel = "";
	if (content.length() - 9 > 0)
	{
		addrLabel = content.substr(9, content.length() - 9);
	}
	if (addrLabel.length() > 0)
	{
		int addr = strtol(addrStr.c_str(), NULL, 16);
		//NM_COMMENT - for comment
		//NM_LABEL   - for label
		Insertname(addr, isLabel ? NM_LABEL : NM_COMMENT, (char*)addrLabel.c_str());
	}
}

void ODBG_Pluginaction(int origin, int action, void *item)
{
	if (origin != PM_MAIN)
		return;

	switch (action)
	{
	case 0:

		char* text = "an.txt";//[TEXTLEN];
		/*if (Gettext("Enter the output file name", text, ' ', 192, FIXEDFONT) <= 1)
			return;*/

		FILE* file;
		fopen_s(&file, text, "r");
		if (file == NULL)
		{
			MessageBox(NULL, "Couldn't find an.txt", "Error", 0);
			return;
		}

		std::string commentHeader = "|AnC|";
		std::string labelHeader = "|AnL|";
		std::string content;
		std::string line;
		bool isLabel = false;
		while (!(line = readLine(file)).empty())
		{
			int anComment = line.find(commentHeader);
			int anLabel = line.find(labelHeader);
			if (anComment == 0)
			{
				if (content.length() > 0)
				{
					handleContent(content, isLabel);
				}
				content = line.substr(commentHeader.length());
				isLabel = false;
			}
			else if (anLabel == 0)
			{
				if (content.length() > 0)
				{
					handleContent(content, isLabel);
				}
				content = line.substr(labelHeader.length());
				isLabel = true;
			}
			else
			{
				content += line;
			}
		}
		if (content.length() > 0)
		{
			handleContent(content, isLabel);
		}
		fclose(file);

		break;
	}
}

BOOL WINAPI DllEntryPoint(HINSTANCE /*hi*/, DWORD /*reason*/, LPVOID /*reserved*/)
{
	return true;
}

void main()
{
	char* text = "an.txt";//[TEXTLEN];
	/*if (Gettext("Enter the output file name", text, ' ', 192, FIXEDFONT) <= 1)
	return;*/

	FILE* file;
	fopen_s(&file, text, "r");
	if (file == NULL)
	{
		//MessageBox(NULL, "Couldn't find an.txt", "Error", 0);
		return;
	}

	std::string commentHeader = "|AnC|";
	std::string labelHeader = "|AnL|";
	std::string content;
	std::string line;
	bool isLabel = false;
	while (!(line = readLine(file)).empty())
	{
		int anComment = line.find(commentHeader);
		int anLabel = line.find(labelHeader);
		if (anComment == 0)
		{
			if (content.length() > 0)
			{
				handleContent(content, isLabel);
			}
			content = line.substr(commentHeader.length());
			isLabel = false;
		}
		if (anLabel == 0)
		{
			if (content.length() > 0)
			{
				handleContent(content, isLabel);
			}
			content = line.substr(labelHeader.length());
			isLabel = true;
		}
		else
		{
			content += line;
		}
	}
	if (content.length() > 0)
	{
		handleContent(content, isLabel);
	}
	fclose(file);
}