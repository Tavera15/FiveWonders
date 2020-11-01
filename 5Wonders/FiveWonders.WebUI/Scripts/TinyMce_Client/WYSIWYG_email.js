tinyMCE.init({
    selector: "#email",
    plugins: [
        'advlist autolink lists image preview anchor',
        'searchreplace visualblocks',
        'table paste help'
    ],
    toolbar: 'undo redo | formatselect | ' +
        'bold italic backcolor | alignleft aligncenter ' +
        'alignright alignjustify | bullist numlist outdent indent | ' +
        'removeformat | help',
    content_style: 'body { font-family:Helvetica,Arial,sans-serif; font-size:14px }'
});