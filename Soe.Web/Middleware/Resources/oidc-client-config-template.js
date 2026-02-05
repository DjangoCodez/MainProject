(function () {
    if (!window.soe) {
        window.soe = {};
    }

    //Oidc.Log.logger = console;
    //Oidc.Log.level = Oidc.Log.ERROR;
    
    var config = {
        authority: "{{authority}}",
        client_id: "{{clientId}}",
        redirect_uri: "{{redirectUri}}",
        post_logout_redirect_uri: "{{postLogoutRedirectUri}}",
        silent_redirect_uri: window.location.protocol + "//" + window.location.host + "/{{silentTokenRenewalRedirectPath}}",
        response_type: "code",
        scope: "openid profile {{scopes}}",
        loadUserInfo: false,
        accesstokenexpiringnotificationtime: 1700
    };

    window.soe.userManager = new Oidc.UserManager(config);
})();