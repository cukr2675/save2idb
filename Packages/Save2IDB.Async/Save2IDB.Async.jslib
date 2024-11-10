const Save2IDBAsyncPlugin = {
    
    $Save2IDB_Async: {
        fs: null,
        callbackFS: null,
        
        callbackText: (callback, ohPtr, text) => {
            const buffer = new TextEncoder().encode(text + String.fromCharCode(0));
            const textPtr = Module._malloc(buffer.length);
            Module.HEAPU8.set(buffer, textPtr);
            Module.dynCall_vii(callback, ohPtr, textPtr);
            Module._free(textPtr);
        }
    },
    
    
    
    Save2IDB_Async_GetDataPath: function () {
        const index = location.pathname.lastIndexOf('/');
        const pathname = location.pathname.substring(0, index);
        const dataPath = encodeURI(location.origin + pathname);
        var bufferSize = lengthBytesUTF8(dataPath) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(dataPath, buffer, bufferSize);
        return buffer;
    },
    
    Save2IDB_Async_Initialize: function (indexedDBNamePtr) {
        const filer = _Save2IDB_Async_requireFiler();
        const indexedDBName = UTF8ToString(indexedDBNamePtr);
        Save2IDB_Async.callbackFS = new filer.FileSystem({ name: indexedDBName });
        Save2IDB_Async.fs = Save2IDB_Async.callbackFS.promises;
    },

    Save2IDB_Async_FileStream_Open: async function (ohPtr, pathPtr, flagsPtr, thenCallback, catchCallback) {
        try {
            const path = UTF8ToString(pathPtr);
            const flags = UTF8ToString(flagsPtr);

            const fd = await Save2IDB_Async.fs.open(path, flags);
            const stats = await Save2IDB_Async.fs.stat(path);
            stats.fd = fd;
            Save2IDB_Async.callbackText(thenCallback, ohPtr, JSON.stringify(stats));
            
        } catch (error) {
            console.error(`Save2IDB_Async_FileStream_Open error: ${error}`);
            Save2IDB_Async.callbackText(catchCallback, ohPtr, error);
        }
    },

    Save2IDB_Async_FileStream_Close: function (fd) {
        try {
            Save2IDB_Async.callbackFS.close(fd);
        } catch (error) {
            console.error(`Save2IDB_Async_FileStream_Close error: ${error}`);
        }
    },

    Save2IDB_Async_FileStream_ReadFile: async function (ohPtr, pathPtr, bytesPtr, bytesLen, thenCallback, catchCallback) {
        try {
            const path = UTF8ToString(pathPtr);
            
            const readBuffer = await Save2IDB_Async.fs.readFile(path);
            Module.HEAPU8.set(readBuffer, bytesPtr);
            Module.dynCall_vi(thenCallback, ohPtr);
            
        } catch (error) {
            console.error(`Save2IDB_Async_FileStream_ReadFile error: ${error}`);
            Save2IDB_Async.callbackText(catchCallback, ohPtr, error);
        }
    },

    Save2IDB_Async_FileStream_WriteFile: async function (ohPtr, pathPtr, bytesPtr, bytesLen, thenCallback, catchCallback) {
        try {
            const path = UTF8ToString(pathPtr);
            const buffer = Module.HEAPU8.subarray(bytesPtr, bytesPtr + bytesLen);
            Object.setPrototypeOf(buffer, Filer.Buffer.prototype); // Convert Uint8Array to Filer.Buffer without allocating buffer.
            
            await Save2IDB_Async.fs.writeFile(path, buffer);
            Module.dynCall_vi(thenCallback, ohPtr);

        } catch (error) {
            console.error(`Save2IDB_Async_FileStream_WriteFile error: ${error}`);
            Save2IDB_Async.callbackText(catchCallback, ohPtr, error);
        }
    },

    Save2IDB_Async_Exists: async function (ohPtr, pathPtr, thenCallback, catchCallback) {
        try {
            const path = UTF8ToString(pathPtr);

            Save2IDB_Async.callbackFS.exists(path, function (exists) {
                Module.dynCall_vii(thenCallback, ohPtr, exists);
            });

        } catch (error) {
            console.error(`Save2IDB_Async_Exists error: ${error}`);
            Save2IDB_Async.callbackText(catchCallback, ohPtr, error);
        }
    },

    Save2IDB_Async_Delete: async function (ohPtr, pathPtr, thenCallback, catchCallback) {
        try {
            const path = UTF8ToString(pathPtr);

            await Save2IDB_Async.fs.unlink(path);
            Module.dynCall_vi(thenCallback, ohPtr);

        } catch (error) {
            console.error(`Save2IDB_Async_Delete error: ${error}`);
            Save2IDB_Async.callbackText(catchCallback, ohPtr, error);
        }
    },

    Save2IDB_Async_Move: async function (ohPtr, sourceFileNamePtr, destFileNamePtr, thenCallback, catchCallback) {
        try {
            const sourceFileName = UTF8ToString(sourceFileNamePtr);
            const destFileName = UTF8ToString(destFileNamePtr);

            await Save2IDB_Async.fs.rename(sourceFileName, destFileName);
            Module.dynCall_vi(thenCallback, ohPtr);

        } catch (error) {
            console.error(`Save2IDB_Async_Move error: ${error}`);
            Save2IDB_Async.callbackText(catchCallback, ohPtr, error);
        }
    },

    Save2IDB_Async_Copy: async function (ohPtr, sourceFileNamePtr, destFileNamePtr, overwrite, thenCallback, catchCallback) {
        try {
            const sourceFileName = UTF8ToString(sourceFileNamePtr);
            const destFileName = UTF8ToString(destFileNamePtr);
            if (!overwrite) {
                const exists = await Save2IDB_Async.fs.exists(path);
                if (exists) throw new Error(`File already exists at ${destFileName}.`);
            }

            const buffer = await Save2IDB_Async.fs.readFile(sourceFileName);
            await Save2IDB_Async.fs.writeFile(destFileName, buffer);
            Module.dynCall_vi(thenCallback, ohPtr);

        } catch (error) {
            console.error(`Save2IDB_Async_Copy error: ${error}`);
            Save2IDB_Async.callbackText(catchCallback, ohPtr, error);
        }
    },

    Save2IDB_Async_GetFileSystemEntries: async function (ohPtr, pathPtr, containFiles, containDirectories, thenCallback, catchCallback) {
        try {
            const path = UTF8ToString(pathPtr);

            const entries = await Save2IDB_Async.fs.readdir(path);
            const filtedEntries = await Promise.all(entries.map(async entry => {
                const stats = await Save2IDB_Async.fs.stat(`${path}/${entry}`);
                if (stats.type === 'FILE' && containFiles) return entry;
                if (stats.type === 'DIRECTORY' && containDirectories) return entry;
                return null;
            }));
            Save2IDB_Async.callbackText(thenCallback, ohPtr, JSON.stringify(filtedEntries));

        } catch (error) {
            console.error(`Save2IDB_Async_GetFiles error: ${error}`);
            Save2IDB_Async.callbackText(catchCallback, ohPtr, error);
        }
    },

    Save2IDB_Async_CreateDirectory: async function (ohPtr, pathPtr, thenCallback, catchCallback) {
        try {
            const path = UTF8ToString(pathPtr);

            await Save2IDB_Async.fs.mkdir(path);
            Module.dynCall_vi(thenCallback, ohPtr);

        } catch (error) {
            console.error(`Save2IDB_Async_CreateDirectory error: ${error}`);
            Save2IDB_Async.callbackText(catchCallback, ohPtr, error);
        }
    },

    Save2IDB_Async_DeleteDirectory: async function (ohPtr, pathPtr, recursive, thenCallback, catchCallback) {
        try {
            const path = UTF8ToString(pathPtr);

            await Save2IDB_Async.fs.rmdir(path);
            Module.dynCall_vi(thenCallback, ohPtr);

        } catch (error) {
            console.error(`Save2IDB_Async_DeleteDirectory error: ${error}`);
            console.error('NOTE: Recursive delete is not implemented.');
            Save2IDB_Async.callbackText(catchCallback, ohPtr, error);
        }
    },

};

autoAddDeps(Save2IDBAsyncPlugin, '$Save2IDB_Async');
mergeInto(LibraryManager.library, Save2IDBAsyncPlugin);
