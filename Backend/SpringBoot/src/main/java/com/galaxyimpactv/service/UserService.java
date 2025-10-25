package com.galaxyimpactv.service;

import com.galaxyimpactv.model.User;
import com.galaxyimpactv.repository.UserRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.security.crypto.bcrypt.BCryptPasswordEncoder;
import org.springframework.stereotype.Service;

import java.util.List;
import java.util.Optional;

@Service // Marca esta clase como un servicio de negocio
@RequiredArgsConstructor // Genera un constructor automático con los atributos final
public class UserService {

    // Inyectamos el repositorio (Spring lo hace automáticamente)
    private final UserRepository userRepository;

    private final BCryptPasswordEncoder passwordEncoder = new BCryptPasswordEncoder();

    // Devuelve todos los usuarios de la base de datos
    public List<User> getAllUsers() {
        return userRepository.findAll();
    }

    // Busca un usuario por su ID
    public Optional<User> getUserById(Integer id) {
        return userRepository.findById(id);
    }

    // Crea o actualiza un usuario
    public User saveUser(User user) {
        // Encripta la contraseña antes de guardar
        String hashedPassword = passwordEncoder.encode(user.getPassword());
        user.setPassword(hashedPassword);

        // Guarda el usuario con la contraseña ya encriptada
        return userRepository.save(user);
    }

    // Elimina un usuario por su ID
    public void deleteUser(Integer id) {
        userRepository.deleteById(id);
    }
}
