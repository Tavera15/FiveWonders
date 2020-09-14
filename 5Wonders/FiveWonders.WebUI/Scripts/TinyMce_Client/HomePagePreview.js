tinyMCE.init({
    selector: "#homeTextPrev",
    plugins: [
        'advlist autolink lists link image charmap print preview anchor',
        'searchreplace visualblocks code fullscreen',
        'insertdatetime media table paste code help wordcount'
    ],
    toolbar: 'undo redo | formatselect | ' +
        'bold italic backcolor | alignleft aligncenter ' +
        'alignright alignjustify | bullist numlist outdent indent | ' +
        'removeformat | help',
    content_style: 'body { font-family:Helvetica,Arial,sans-serif; font-size:14px }',
    setup: function (editor) {
        editor.on('Paste Change input Undo Redo', function () {
            var destination = document.getElementById('home-prev-text');
            var content = tinyMCE.activeEditor.getContent();

            destination.innerHTML = content;
        });
    }
});