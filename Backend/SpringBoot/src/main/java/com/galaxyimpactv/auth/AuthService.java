package com.galaxyimpactv.auth;

import com.galaxyimpactv.model.User;
import com.galaxyimpactv.repository.UserRepository;
import org.springframework.security.crypto.bcrypt.BCryptPasswordEncoder;
import org.springframework.stereotype.Service;

/**
 * Servicio que valida las credenciales de un usuario.
 */
@Service
public class AuthService {

    private final UserRepository userRepository;
    private final BCryptPasswordEncoder passwordEncoder;

    public AuthService(UserRepository userRepository) {
        this.userRepository = userRepository;
        this.passwordEncoder = new BCryptPasswordEncoder();
    }

    /**
     * Verifica si el usuario existe y si la contraseña es válida.
     */
    public boolean login(String username, String password) {
        // Busca el usuario por nombre (usa tu método existente)
        User user = userRepository.findByUsername(username);

        // Si no existe, login inválido
        if (user == null) {
            return false;
        }

        // Compara la contraseña ingresada con el hash guardado
        return passwordEncoder.matches(password, user.getPassword());
    }
}
