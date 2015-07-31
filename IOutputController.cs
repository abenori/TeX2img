using System;
using System.Collections.Generic;
using System.Text;


namespace TeX2img
{
    interface IOutputController
    {
        void showExtensionError(string file);
        void showPathError(string exeName, string necessary);
        void appendOutput(string log);
        void showGenerateError();
        void showImageMagickError();
        void scrollOutputTextBoxToEnd();
        void showUnauthorizedError(string filePath);
        void showIOError(string filePath);
		bool askYesorNo(string msg);
        void showToolError(string tool);
        void errorIgnoredWarning();
    }
}
