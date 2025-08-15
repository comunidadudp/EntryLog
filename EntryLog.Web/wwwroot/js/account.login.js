(function () {
    'use strict'

    const loginForm = document.getElementById('login-form');

    loginForm.addEventListener('submit', function (event) {
        event.preventDefault();

        if (!loginForm.checkValidity()) {
            loginForm.classList.add('was-validated');
        } else {
            const formInputs = loginForm.querySelectorAll('input');

            let model = {};

            formInputs.forEach((element, index) => {
                model[element.name] = element.value;
            });

            $.ajax({
                url: '/account/login',
                type: 'POST',
                async: true,
                data: {
                    __RequestVerificationToken: model.__RequestVerificationToken,
                    model
                },
                beforeSend: () => {
                    $('#username').attr('disabled', true);
                    $('#password').attr('disabled', true);
                    $('#login-form-button').html(`
                        <button class="btn btn-dark w-100 mt-4 mb-3" type="button" disabled>
                            <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
                            Ingresando...
                        </button>
                    `);
                },
                complete: () => {
                    $('#username').removeAttr('disabled');
                    $('#password').removeAttr('disabled');
                    $('#login-form-button').html(`
                        <button type="submit" class="btn btn-dark w-100 mt-4 mb-3">Ingresar</button>
                    `);
                },
                success: (result) => {
                    if (result.success) {
                        const location = window.location;
                        const url = `${location.protocol}//${location.host}${result.path}`;
                        window.location.href = url;
                    } else {
                        $.notify({
                            // options
                            icon: 'icon-bell',
                            title: 'Notificación',
                            message: result.message,
                        }, {
                            // settings
                            type: "warning",
                        });
                    }
                },
                error: (err) => {
                    $.notify({
                        // options
                        icon: 'icon-bell',
                        title: 'Notificación',
                        message: 'Ha ocurrido un error en la aplicación',
                    }, {
                        // settings
                        type: "danger",
                    });
                }
            })
        }
    });
})();