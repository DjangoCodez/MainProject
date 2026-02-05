var TermManager =
{
    urlPrefix: '',
    delay: 1000,
    maxValidations: 5,

    validateSysTerms: function (sysTerms, sender)
    {  
        for (var i = 0; i < sysTerms.length; i++)
        {
            var sysTerm = sysTerms[i];
            if (sysTerm.loaded == false || sysTerm.name == '')
                return false;
        }

        return true;
    },

    reachedMaxValidations: function(validations)
    {
        if (validations >= TermManager.maxValidations)
            return true;
        return false;
    },

    createSysTerm: function(sysTermGroupId, sysTermId, defaultTerm)
    {
        var sysTerm = new Object();
        sysTerm.sysTermId = sysTermId;
        sysTerm.sysTermGroupId = sysTermGroupId;
        sysTerm.defaultTerm = defaultTerm;
        sysTerm.name = '';
        sysTerm.loaded = false;

        TermManager.loadSysTerm(sysTerm);

        return sysTerm;
    },

    loadSysTerm: function(sysTerm)
    {
        if (TermManager.urlPrefix == '')
            TermManager.urlPrefix = TermManager.getRelativePath(location.pathname) + 'ajax/getSysTerm.aspx';

        var url = TermManager.urlPrefix + '?sysTermId=' + sysTerm.sysTermId + '&sysTermGroupId=' + sysTerm.sysTermGroupId + '&timestamp=' + new Date().getTime(); //make each request unique to prevent cache
        DOMAssistant.AJAX.get(url, function (data, status)
        {
            var obj = JSON.parse(data);
            if (obj && obj.Found)
            {
                sysTerm.loaded = true;
                sysTerm.name = obj.Name;
            }
        });          
    },

    getText: function(sysTerms, sysTermGroupId, sysTermId)
    {
        for (var i = 0; i < sysTerms.length; i++)
        {
            var sysTerm = sysTerms[i];
            if (sysTerm.loaded && sysTerm.sysTermId == sysTermId && sysTerm.sysTermGroupId == sysTermGroupId)
                return sysTerm.name;
        }
        return '';
    },

    getRelativePath: function (path)
    {
        var count = 0;
        var relative = "";
        for (var i = 0; i < path.length; i++)
        {
            if (path.substr(i, 1) == "/")
                count++;
        }
        for (var j = 1; j < count; j++)
        {
            relative += "../";
        }
        return relative;
    }
}