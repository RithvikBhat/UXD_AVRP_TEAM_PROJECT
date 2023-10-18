using SFB;
using System;
using System.Collections.Generic;

namespace HCIG.UsefulTools {

    /// <summary>
    /// Intermediate layer that communicates with the integrated asset/plugin and serves as our sole interaction interface
    /// </summary>
    public class FileBrowserManager : Singleton<FileBrowserManager> {



        /// <summary>
        /// Native open file dialog
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="directory">Root directory</param>
        /// <param name="extension">Allowed extension</param>
        /// <param name="multiselect">Allow multiple file selection</param>
        /// <returns>Returns array of chosen paths. Zero length array when cancelled</returns>
        public string[] OpenFilePanel(string title, string directory, string extension, bool multiselect) {
            return StandaloneFileBrowser.OpenFilePanel(title, directory, extension, multiselect);
        }

        /// <summary>
        /// Native open file dialog
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="directory">Root directory</param>
        /// <param name="extensions">List of extension filters. Filter Example: new ExtensionFilter("Image Files", "jpg", "png")</param>
        /// <param name="multiselect">Allow multiple file selection</param>
        /// <returns>Returns array of chosen paths. Zero length array when cancelled</returns>
        public string[] OpenFilePanel(string title, string directory, ExtensionFilter[] extensions, bool multiselect) {
            return StandaloneFileBrowser.OpenFilePanel(title, directory, ConvertExtensions(extensions), multiselect);
        }

        /// <summary>
        /// Native open file dialog async
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="directory">Root directory</param>
        /// <param name="extension">Allowed extension</param>
        /// <param name="multiselect">Allow multiple file selection</param>
        /// <param name="cb">Callback")</param>
        public void OpenFilePanelAsync(string title, string directory, string extension, bool multiselect, Action<string[]> cb) {
            StandaloneFileBrowser.OpenFilePanelAsync(title, directory, extension, multiselect, cb);
        }

        /// <summary>
        /// Native open file dialog async
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="directory">Root directory</param>
        /// <param name="extensions">List of extension filters. Filter Example: new ExtensionFilter("Image Files", "jpg", "png")</param>
        /// <param name="multiselect">Allow multiple file selection</param>
        /// <param name="cb">Callback")</param>
        public void OpenFilePanelAsync(string title, string directory, ExtensionFilter[] extensions, bool multiselect, Action<string[]> cb) {
            StandaloneFileBrowser.OpenFilePanelAsync(title, directory, ConvertExtensions(extensions), multiselect, cb);
        }

        /// <summary>
        /// Native open folder dialog
        /// NOTE: Multiple folder selection doesn't supported on Windows
        /// </summary>
        /// <param name="title"></param>
        /// <param name="directory">Root directory</param>
        /// <param name="multiselect"></param>
        /// <returns>Returns array of chosen paths. Zero length array when cancelled</returns>
        public string[] OpenFolderPanel(string title, string directory, bool multiselect) {
            return StandaloneFileBrowser.OpenFolderPanel(title, directory, multiselect);
        }

        /// <summary>
        /// Native open folder dialog async
        /// NOTE: Multiple folder selection doesn't supported on Windows
        /// </summary>
        /// <param name="title"></param>
        /// <param name="directory">Root directory</param>
        /// <param name="multiselect"></param>
        /// <param name="cb">Callback")</param>
        public void OpenFolderPanelAsync(string title, string directory, bool multiselect, Action<string[]> cb) {
            StandaloneFileBrowser.OpenFolderPanelAsync(title, directory, multiselect, cb);
        }

        /// <summary>
        /// Native save file dialog
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="directory">Root directory</param>
        /// <param name="defaultName">Default file name</param>
        /// <param name="extension">File extension</param>
        /// <returns>Returns chosen path. Empty string when cancelled</returns>
        public string SaveFilePanel(string title, string directory, string defaultName, string extension) {
            return StandaloneFileBrowser.SaveFilePanel(title, directory, defaultName, extension);
        }

        /// <summary>
        /// Native save file dialog
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="directory">Root directory</param>
        /// <param name="defaultName">Default file name</param>
        /// <param name="extensions">List of extension filters. Filter Example: new ExtensionFilter("Image Files", "jpg", "png")</param>
        /// <returns>Returns chosen path. Empty string when cancelled</returns>
        public string SaveFilePanel(string title, string directory, string defaultName, ExtensionFilter[] extensions) {
            return StandaloneFileBrowser.SaveFilePanel(title, directory, defaultName, ConvertExtensions(extensions));
        }

        /// <summary>
        /// Native save file dialog async
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="directory">Root directory</param>
        /// <param name="defaultName">Default file name</param>
        /// <param name="extension">File extension</param>
        /// <param name="cb">Callback")</param>
        public void SaveFilePanelAsync(string title, string directory, string defaultName, string extension, Action<string> cb) {
            StandaloneFileBrowser.SaveFilePanelAsync(title, directory, defaultName, extension, cb);
        }

        /// <summary>
        /// Native save file dialog async
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="directory">Root directory</param>
        /// <param name="defaultName">Default file name</param>
        /// <param name="extensions">List of extension filters. Filter Example: new ExtensionFilter("Image Files", "jpg", "png")</param>
        /// <param name="cb">Callback")</param>
        public void SaveFilePanelAsync(string title, string directory, string defaultName, ExtensionFilter[] extensions, Action<string> cb) {



            StandaloneFileBrowser.SaveFilePanelAsync(title, directory, defaultName, ConvertExtensions(extensions), cb);
        }

        #region Conversions

        /// <summary>
        /// Converts our filter into the variant the SFB needs
        /// </summary>
        /// <param name="extensions"></param>
        /// <returns></returns>
        private SFB.ExtensionFilter[] ConvertExtensions(ExtensionFilter[] extensions) {

            List<SFB.ExtensionFilter> extensionsSFB = new List<SFB.ExtensionFilter>();

            foreach (ExtensionFilter extension in extensions) {
                extensionsSFB.Add(new SFB.ExtensionFilter(extension.Name, extension.Extensions));
            }

            return extensionsSFB.ToArray();
        }

        #endregion
    }

    public struct ExtensionFilter {
        public string Name;
        public string[] Extensions;

        public ExtensionFilter(string filterName, params string[] filterExtensions) {
            Name = filterName;
            Extensions = filterExtensions;
        }
    }
}
