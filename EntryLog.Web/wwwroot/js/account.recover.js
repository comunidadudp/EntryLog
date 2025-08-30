(() => {
    'use strict'

    const recoverForm = document.getElementById('recover-start-form');

    recoverForm.addEventListener('submit', function (event) {
        event.preventDefault();

        if (!recoverForm.checkValidity()) {
            recoverForm.classList.add('was-validated');
        } else {
            const email = recoverForm.querySelector('#email').value;

            $.ajax({
                url: '/cuenta/recuperar',
                type: 'POST',
                async: true,
                data: {
                    email
                },
                beforeSend: () => {
                    $('#email').attr('disabled', true);
                    $('#recover-start-form-btn').html(`
                        <button class="btn btn-dark w-100 mt-4 mb-3" type="button" disabled>
                            <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
                            Validando...
                        </button>
                    `);
                },
                complete: () => {
                    $('#email').removeAttr('disabled');
                    $('#recover-start-form-btn').html(`
                        <button type="submit" class="btn btn-dark w-100 mt-4 mb-3">Recuperar cuenta</button>
                    `);
                },
                success: (result) => {
                    $.notify({
                        icon: 'icon-bell',
                        title: 'Notificación',
                        message: result.message,
                    }, {
                        type: result.success ? 'success' : 'warning',
                    });
                },
                error: (err) => {
                    $.notify({
                        icon: 'icon-bell',
                        title: 'Notificación',
                        message: err.message,
                    }, {
                        type: 'danger',
                    });
                }
            })


        }

    })

})();