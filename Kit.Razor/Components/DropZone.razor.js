window._callbacker = function (callbackObjectInstance, callbackMethod, callbackId, cmd, args) {
    var parts = cmd.split('.');
    var targetFunc = window;
    var parentObject = window;
    for (var i = 0; i < parts.length; i++) {
        if (i == 0 && part == 'window') continue;
        var part = parts[i];
        parentObject = targetFunc;
        targetFunc = targetFunc[part];
    }
    args = JSON.parse(args);
    args.push(function (e, d) {
        var args = [];
        for (var i in arguments) args.push(JSON.stringify(arguments[i]));
        callbackObjectInstance.invokeMethodAsync(callbackMethod, callbackId, args);
    });
    targetFunc.apply(parentObject, args);
};

window.InitDropZone = function (callback) {

    $("#container").on("dragover", function (e) {
        e.preventDefault();
        $(this).addClass("file-over");
        //$('svg path').show();
    });

    $("#container").on("dragleave", function (e) {
        e.preventDefault();
        e.stopPropagation();
        $(this).removeClass("file-over");
    });

    $("#container").on("drop", function (e) {
        e.preventDefault();
        e.stopPropagation();
        $(this).addClass("file-over").stop(true, true).css({
            background: "#fff"
        });
        $(".progress").toggleClass("complete");
        $("#image-holder").addClass("move");
    });

    var dropzone = document.getElementById("container");

    FileReaderJS.setupDrop(dropzone, {
        readAsDefault: "DataURL",
        on: {
            load: function (e, file) {
                //var img = document.getElementById("image-holder");
                //img.onload = function () {
                //    document.getElementById("image-holder").appendChild(img);
                //};
                //img.src = e.target.result;
                callback(file);
            }
        }
    });
}
