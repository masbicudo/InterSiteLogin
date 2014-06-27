if (!window.Login) {

    window.Login = {
        processLoginResponse: function (response) {
            window.location = response;
        }
    };

}