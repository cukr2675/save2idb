using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Save2IDB.Async
{
    /// <summary>
    /// https://github.com/filerjs/filer/tree/master?tab=readme-ov-file#stat
    /// </summary>
    [System.Serializable]
    internal class WebGLFileStats
    {
        /// <summary>
        /// [Additional] file descriptor
        /// </summary>
        public int fd = 0;

        /// <summary>
        /// internal node id (unique)
        /// </summary>
        public string node = null;

        /// <summary>
        /// file system name
        /// </summary>
        public string dev = null;

        /// <summary>
        /// the entry's name (basename)
        /// </summary>
        public string name = null;

        /// <summary>
        /// file size in bytes
        /// </summary>
        public int size = 0;

        /// <summary>
        /// number of links
        /// </summary>
        public int nlinks = 0;

        /// <summary>
        /// last access time as JS Date Object
        /// </summary>
        public System.DateTime atime = System.DateTime.MinValue;

        /// <summary>
        /// last modified time as JS Date Object
        /// </summary>
        public System.DateTime mtime = System.DateTime.MinValue;

        /// <summary>
        /// creation time as JS Date Object
        /// </summary>
        public System.DateTime ctime = System.DateTime.MinValue;

        /// <summary>
        /// last access time as Unix Timestamp
        /// </summary>
        public long atimeMs = 0;

        /// <summary>
        /// last modified time as Unix Timestamp
        /// </summary>
        public long mtimeMs = 0;

        /// <summary>
        /// creation time as Unix Timestamp
        /// </summary>
        public long ctimeMs = 0;

        /// <summary>
        /// file type (FILE, DIRECTORY, SYMLINK)
        /// </summary>
        public string type = null;

        /// <summary>
        /// group name
        /// </summary>
        public int gid = 0;

        /// <summary>
        /// owner name
        /// </summary>
        public int uid = 0;

        /// <summary>
        /// permissions
        /// </summary>
        public int mode = 0;

        ///// <summary>
        ///// version of the node
        ///// </summary>
        //public object version = null;
    }
}
