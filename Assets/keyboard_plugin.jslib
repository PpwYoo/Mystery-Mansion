mergeInto(LibraryManager.library, {
    ShowPrompt: function (msg) {
        var result = prompt(UTF8ToString(msg), "");
        if (result === null) result = "";
        var buffer = allocate(intArrayFromString(result), 'i8', ALLOC_NORMAL);
        return buffer;
    }
});