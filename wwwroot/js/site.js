$(document).ready(function () {
  // $("#sidebar").mCustomScrollbar({
  //     theme: "minimal"
  // });

  // Toggling Blog filter
  $('#sidebarCollapse').on('click', function () {
      $('#sidebar, #content').toggleClass('active');
      // $('.collapse.in').toggleClass('in');
      // $('a[aria-expanded=true]').attr('aria-expanded', 'false');
  });

  // Lightbox image gallery
  $(document).on('click', '[data-toggle="lightbox"]', function(event) {
    event.preventDefault();
    $(this).ekkoLightbox();
  });

  // Animation homepage section title
  $('.slide-in').animate({left: '0px', opacity: 1}, 2000);

  // Importing images on Blogs and Projects Create
  $(".custom-file-input").on("change", function () {
    var fileLabel = $(this).next(".custom-file-label");
    var files = $(this)[0].files;
    if (files.length > 1){
      fileLabel.html(files.length + " files selected");
    }
    else if (files.length == 1){
      fileLabel.html(files[0].name);
    }
  });
});