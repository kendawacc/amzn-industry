var Frontend =
{
    JavaScript_Initialize: function () {
        document.getElementById("SplashContainer").style.display = "none";
        document.getElementById("keyboard").oninput = function () {
            SendMessage('Canvas', 'ReceiveInputChange', document.getElementById("keyboard").value);
        };
        document.getElementById("keyboard").onblur = function () {
            SendMessage('Canvas', 'LoseFocus');
        };
    },

    JavaScript_auditDevice: function () {
        var device = "Unknown OS";
        if (!/iPhone|iPad|iPod|Android/i.test(navigator.userAgent)) {
            device = "Desktop";
        } else {
            device = "Mobile";
        }
        SendMessage('Canvas', 'Audit', device);
    },

    JavaScript_DocumentCookie: function (Type, Data) {
        var type = UTF8ToString(Type);
        var data = UTF8ToString(Data);

        switch (type) {
            case "Set":
                var cookiedate = new Date();
                cookiedate.setFullYear(cookiedate.getFullYear() + 50);

                document.cookie =
                    "COOKIE_DATA=" + data +
                    "; expires=" + cookiedate.toUTCString() +
                    "; path=/; Secure; SameSite=Lax";
                break;
            case "Get":
                var cookieindex = '';
                var cookies = document.cookie.split(";");
                cookies.forEach(function (cookie) {
                    if (cookie.slice(0, cookie.indexOf('=')) === "COOKIE_DATA") {
                        cookieindex = cookie.replace("COOKIE_DATA=", "").toString();
                    }
                });
                SendMessage('Canvas', 'JavaScript_DocumentCookie_Internal', cookieindex);
                break;
            case "Delete":
                document.cookie = "COOKIE_DATA=" + ";max-age=0";
                break;
        }
    },

    JavaScript_WindowLocationHash: function () {
        SendMessage('Canvas', 'JavaScript_WindowLocationHash_Internal', window.location.hash);
    },

    JavaScript_WindowPrompt: function (Message) {
        var message = UTF8ToString(Message);
        navigator.clipboard.writeText(message);
    },

    JavaScript_WindowClipboard: function (Clip) {
        var clip = UTF8ToString(Clip);
        navigator.permissions.query({ name: 'clipboard-write' });
        navigator.clipboard.writeText(clip);
    },

    JavaScript_Redirect: function (Url) {
        var url
            = UTF8ToString(Url);

        window
            .location
            .href
            = url;
    },

    JavaScript_OpenURL: function (Url) {
        var url = UTF8ToString(Url);
        if (!/iPhone|iPad|iPod|Android/i.test(navigator.userAgent)) {
            window.open(url, '_blank');
        } else {
            window.open(url, '_system');
        }
    },

    JavaScript_WindowHistoryPushState: function (Title, LocationHash) {
        var title = UTF8ToString(Title);
        var locationhash = UTF8ToString(LocationHash);
        document.title = title;
        document.location.hash = locationhash + '/';
    },

    JavaScript_DownloadBlob: function (DataPtr, Length, Name, Mime) {
        const name = UTF8ToString(Name);
        const mime = UTF8ToString(Mime);

        const bytes = new Uint8Array(Module.HEAPU8.buffer, DataPtr, Length);
        const blob = new Blob([bytes], { type: mime });

        const url = URL.createObjectURL(blob);

        const a = document.createElement("a");
        a.href = url;
        a.download = name;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);

        URL.revokeObjectURL(url);
    },

    JavaScript_WindowFileDialog: function (Callback, Attribute) {
        var callback = UTF8ToString(Callback);
        var attribute = UTF8ToString(Attribute);
        var chooser = document.querySelector('#WindowFileDialog');
        chooser.addEventListener("change", function (evt) {
            var array = [];
            var files = evt.target.files;
            for (var i = 0; i < files.length; i++) {
                array.push({
                    callback: callback,
                    blobname: files[i].name,
                    blobextension: "." + files[i].name.split('.').pop(),
                    bloburl: window.URL.createObjectURL(files[i])
                });
            }
            SendMessage('Canvas', 'FileDialog_Internal', JSON.stringify(array));
            chooser.removeEventListener("change", evt);
        }, false);
        chooser.setAttribute('accept', attribute);
        chooser.click();
    },

    JavaScript_RegisterPasskey: function (Domain, User, Auth) {
        (async () => {

            const domain
                = UTF8ToString
                    (Domain);

            const user
                = UTF8ToString
                    (User);

            const auth
                = UTF8ToString
                    (Auth);

            function JavaScript_ToBase64Url(buffer) {
                return btoa(String.fromCharCode(...new Uint8Array(buffer)))
                    .replace(/\+/g, "-")
                    .replace(/\//g, "_")
                    .replace(/=/g, "");
            }

            function JavaScript_FromBase64Url(base64url) {
                const base64 = base64url.replace(/-/g, "+").replace(/_/g, "/");
                const padded = base64 + "=".repeat((4 - base64.length % 4) % 4);
                return Uint8Array.from(atob(padded), c => c.charCodeAt(0)).buffer;
            }

            try {
                const optionsResponse
                    = await fetch(`https://${domain}/Auth/Register/Options`, {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify({
                            username: user,
                            auth: auth,
                        })
                    });

                if (!optionsResponse.ok) {
                    const text = await optionsResponse.text();
                    throw new Error(`Options failed (${optionsResponse.status}): ${text}`);
                }

                const publicKeyOptions
                    = await optionsResponse.json();

                publicKeyOptions.challenge
                    = JavaScript_FromBase64Url(publicKeyOptions.challenge);

                publicKeyOptions.user.id
                    = JavaScript_FromBase64Url(publicKeyOptions.user.id);

                if (publicKeyOptions.excludeCredentials) {
                    publicKeyOptions.excludeCredentials
                        = publicKeyOptions.excludeCredentials
                            .map(cred => ({
                                ...cred,
                                id: JavaScript_FromBase64Url(cred.id)
                            }));
                }

                const credential
                    = await navigator
                        .credentials
                        .create({ publicKey: publicKeyOptions });

                if (!credential) {
                    throw new Error
                        ("Credential creation returned null");
                }

                const result = {
                    id: credential.id,
                    rawId: JavaScript_ToBase64Url(credential.rawId),
                    type: credential.type,
                    response: {
                        attestationObject: JavaScript_ToBase64Url(
                            credential
                                .response
                                .attestationObject
                        ),
                        clientDataJSON: JavaScript_ToBase64Url(
                            credential
                                .response
                                .clientDataJSON
                        )
                    }
                };

                const verifyResponse
                    = await fetch(`https://${domain}/Auth/Register/Verify`, {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify({
                            attestation: result,
                            username: user
                        })
                    });

                if (!verifyResponse.ok) {
                    const text = await verifyResponse.text();
                    throw new Error
                        (`Verify failed (${verifyResponse.status}): ${text}`);
                }

                const verifyResult
                    = await verifyResponse
                        .json();

                SendMessage(
                    'Canvas',
                    'JavaScript_RegisterPasskey_Internal',
                    JSON
                        .stringify
                        (verifyResult)
                );

            } catch (e) {
                console
                    .error
                    ("Register passkey error:", e);

                SendMessage(
                    'Canvas',
                    'JavaScript_RegisterPasskey_Error_Internal',
                    e.toString()
                );
            }

        })();
    },

    JavaScript_LoginPasskey: function (Domain, User) {
        (async () => {

            const domain
                = UTF8ToString
                    (Domain);

            const user
                = UTF8ToString
                    (User);

            function JavaScript_ToBase64Url(buffer) {
                return btoa(String.fromCharCode(...new Uint8Array(buffer)))
                    .replace(/\+/g, "-")
                    .replace(/\//g, "_")
                    .replace(/=/g, "");
            }

            function JavaScript_FromBase64Url(base64url) {
                const base64 = base64url.replace(/-/g, "+").replace(/_/g, "/");
                const padded = base64 + "=".repeat((4 - base64.length % 4) % 4);
                return Uint8Array.from(atob(padded), c => c.charCodeAt(0)).buffer;
            }

            try {

                const optionsResponse
                    = await fetch(`https://${domain}/Auth/Login/Options`, {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify({ username: user })
                    });

                if (!optionsResponse.ok) {
                    const text = await optionsResponse.text();
                    throw new Error(`Options failed (${optionsResponse.status}): ${text}`);
                }

                const options
                    = await optionsResponse
                        .json();

                options.challenge
                    = JavaScript_FromBase64Url
                        (options.challenge);

                if (options.allowCredentials) {
                    options.allowCredentials
                        = options
                            .allowCredentials.map(c => ({
                                ...c,
                                id: JavaScript_FromBase64Url(c.id)
                            }));
                }

                const assertion
                    = await navigator
                        .credentials.get({
                            publicKey: options
                        });

                if (!assertion) {
                    throw new Error
                        ("Assertion returned null");
                }

                const result = {
                    id: assertion.id,
                    rawId: JavaScript_ToBase64Url(assertion.rawId),
                    type: assertion.type,
                    response: {
                        authenticatorData: JavaScript_ToBase64Url(
                            assertion.response.authenticatorData
                        ),
                        clientDataJSON: JavaScript_ToBase64Url(
                            assertion.response.clientDataJSON
                        ),
                        signature: JavaScript_ToBase64Url(
                            assertion.response.signature
                        ),
                        userHandle: assertion.response.userHandle
                            ? JavaScript_ToBase64Url(assertion.response.userHandle)
                            : null
                    }
                };

                const verifyResponse
                    = await fetch(`https://${domain}/Auth/Login/Verify`, {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify({
                            assertion: result,
                            username: user
                        })
                    });

                if (!verifyResponse.ok) {
                    const text = await verifyResponse.text();
                    throw new Error(`Verify failed (${verifyResponse.status}): ${text}`);
                }

                const verifyResult
                    = await verifyResponse
                        .json();

                SendMessage(
                    'Canvas',
                    'JavaScript_LoginPasskey_Internal',
                    JSON
                        .stringify
                        (verifyResult)
                );

            } catch (e) {
                console
                    .error
                    ("Login passkey error:", e);

                SendMessage(
                    'Canvas',
                    'JavaScript_LoginPasskey_Error_Internal',
                    e.toString()
                );
            }

        })();
    },

    JavaScript_ClientAgent: function (Json) {
        const json
            = UTF8ToString
                (Json);

        ClientAgent
            (json);
    },

    OpenInputKeyboard: function (currentValue) {
        if (!/iPhone|iPad|iPod|Android/i.test(navigator.userAgent)) {
            document.getElementById("keyboard").value = UTF8ToString(currentValue);
            document.getElementById("keyboard").focus();
        } else {
            var keyboard
                = window
                    .prompt("Please enter your information",
                        UTF8ToString(currentValue));

            if (keyboard !== null) {
                SendMessage('Canvas', 'ReceiveInputChange', keyboard);
            }
        }
    },

    CloseInputKeyboard: function () {
        document.getElementById("keyboard").blur();
    }
};
mergeInto(LibraryManager.library, Frontend);