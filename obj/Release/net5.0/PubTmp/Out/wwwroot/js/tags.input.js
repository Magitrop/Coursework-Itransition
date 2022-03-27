$('#input-tags').amsifySuggestags({
    type: 'amsify',
    suggestMatch: function (suggestionItem, value) {
        return ~suggestionItem.toString().toLowerCase().indexOf(value.toString().toLowerCase());
    },
    printValues: false,
    suggestionsAction: {
        timeout: -1,
        minChars: 2,
        minChange: -1,
        delay: 100,
        type: 'POST',
        url: '/CreateUserReview?handler=Tags',
        beforeSend: function (xhr) {
            xhr.setRequestHeader("XSRF-TOKEN",
                $('input:hidden[name="__RequestVerificationToken"]').val());
        },
    }
});
$('.amsify-suggestags-list').addClass('navbar-themed');