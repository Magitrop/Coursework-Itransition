$('#group-files').on('change', 'input[type=file]', function () {
    filesCount++;
    $(this)
        .parent()
        .append('<div id="uploaded-file-' + filesCount + '"><img /></div>')
        .children('#uploaded-file-' + filesCount)
        .css({
            'position': 'relative'
        })
        .append('<button id="remove-image-' + filesCount + '" type="button">&#10005;</button>')
        .children('button')
        .addClass('btn btn-light btn-outline-dark')
        .data('file-index', filesCount)
        .css({
            'position': 'absolute',
            'top': '10px',
            'right': '26%',
        })
        .on("click", function (e) {
            $('#File-' + $(this).data('file-index')).remove();
            $(this).parent().remove();
        });

    var reader = new FileReader();
    reader.onload = function (e) {
        $('#uploaded-file-' + filesCount + ' img')
            .attr("src", e.target.result)
            .css('width', '75%');
    }
    reader.readAsDataURL($(this)[0].files[0]);
    $(this).css('display', 'none');

    $(this).parent().append(
        '<input name="File-' + (filesCount + 1) +
        '" id="File-' + (filesCount + 1) +
        '" type="file" class="form-control-file" accept="image/*" />');
})