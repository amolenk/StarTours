function appendToStream(streamId, expectedVersion, serializedEvents) {
    var events = JSON.parse(serializedEvents);
    var newVersion = expectedVersion + events.length;

    if (expectedVersion === 0) {
        tryCreateVersionDocument(versionDocumentCallback);
    }
    else {
        tryUpdateVersionDocument(versionDocumentCallback);
    }

    function versionDocumentCallback(versionDocumentResult) {
        if (versionDocumentResult) {
            createEventDocuments(function () { __.response.setBody(true); });
        }
        else {
            __.response.setBody(false);
        }
    }

    function tryCreateVersionDocument(onCompletedCallback) {

        var versionDocument =
        {
            id: "version",
            streamId: streamId,
            version: newVersion
        };

        var isAccepted = __.createDocument(__.getSelfLink(), versionDocument, function (err) {
            if (err) {
                if (err.number == 409) { // Conflict
                    onCompletedCallback(false);
                }
                else {
                    throw err;
                }
            }
            else {
                onCompletedCallback(true);
            }
        });

        if (!isAccepted) throw new Error("Error adding version document.");
    }

    function tryUpdateVersionDocument(onCompletedCallback) {

        var isAccepted = __.filter(
            function (x) { return x.id === "version" && x.streamId === streamId },
            function (err, results, options) {
                if (err) throw err;

                if (!results || !results.length) throw new Error("Failed to find the version document.");

                var versionDocument = results[0];

                if (versionDocument.version === expectedVersion) {
                    versionDocument.version = newVersion;

                    var isAccepted = __.replaceDocument(versionDocument._self, versionDocument, function (err) {
                        if (err) throw err;
                        onCompletedCallback(true);
                    });

                    if (!isAccepted) throw new Error("Failed to update version document.");
                }
                else {
                    onCompletedCallback(false);
                }

            });

        if (!isAccepted) throw new Error("Error retrieving metadata document.");
    }

    function createEventDocuments(onCompletedCallback) {
        var index = 0;
        var version = expectedVersion + 1;

        createEventDocument(callback);

        function createEventDocument(callback) {
            var eventDocument = events[index];

            var isAccepted = __.createDocument(__.getSelfLink(), eventDocument, callback);
            if (!isAccepted) throw new Error("Error creating event document.");
        }

        function callback(err, doc, options) {
            if (err) throw err;

            index++;
            version++;

            if (index < events.length) {
                createEventDocument(callback);
            }
            else {
                onCompletedCallback();
            }
        }
    }
}
