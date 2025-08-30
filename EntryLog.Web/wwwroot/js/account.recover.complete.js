(() => {
    'use strict'

    const completeRecoverForm = document.getElementById('recover-complete-form');

    completeRecoverForm.addEventListener('submit', function (event) {
        event.preventDefault();

        if (!completeRecoverForm.checkValidity()) {
            completeRecoverForm.classList.add('was-validated');
        } else {
            const formInputs = completeRecoverForm.querySelectorAll('input');

            let model = {};

            const url = new URL(window.location.href);
            const token = url.searchParams.get('token');
            model['token'] = token;

            formInputs.forEach((element, index) => {
                model[element.name] = element.value;
            });

            $.ajax({
                url: '/cuenta/completar_recuperar',
                type: 'POST',
                async: true,
                data: {
                    model
                },
                beforeSend: () => {
                    $('#password').attr('disabled', true);
                    $('#passwordConf').attr('disabled', true);
                    $('#recover-complete-form-btn').html(`
                        <button class="btn btn-dark w-100 mt-4 mb-3" type="button" disabled>
                            <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
                            Validando...
                        </button>
                    `);
                },
                complete: () => {
                    $('#password').removeAttr('disabled');
                    $('#passwordConf').removeAttr('disabled');
                    $('#recover-complete-form-btn').html(`
                        <button type="submit" class="btn btn-dark w-100 mt-4 mb-3">Guardar</button>
                    `);
                },
                success: (result) => {
                    swal(result.success ? 'Notificación' : 'Oops!', result.message, {
                        icon: result.success ? 'success' : 'warning',
                        buttons: {
                            confirm: {
                                className: `btn btn-${result.success ? 'success' : 'warning'}`
                            }
                        },
                    }).then(() => {
                        if (result.success) {
                            const location = window.location;
                            const url = `${location.protocol}//${location.host}`;
                            window.location.href = url;
                        }
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