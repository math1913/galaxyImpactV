package com.galaxyimpactv.controller;

import com.galaxyimpactv.model.User;
import com.galaxyimpactv.service.UserService;
import lombok.RequiredArgsConstructor;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController // Indica que esta clase define endpoints REST
@RequestMapping("/api/users") // Ruta base: http://localhost:8080/api/users
@RequiredArgsConstructor // Genera el constructor con los atributos final (evista hacer this.userService = userService, etc)
public class UserController {

    private final UserService userService;

    // GET /api/users -> Lista todos los usuarios
    @GetMapping
    public ResponseEntity<List<User>> getAllUsers() {
        List<User> users = userService.getAllUsers();
        return ResponseEntity.ok(users); // Devuelve lista + código HTTP 200
    }

    // GET /api/users/{id} -> Devuelve un usuario por ID
    @GetMapping("/{id}")
    public ResponseEntity<User> getUserById(@PathVariable Integer id) {
        return userService.getUserById(id)
                .map(ResponseEntity::ok)
                .orElse(ResponseEntity.notFound().build()); // 404 si no existe
    }

    // POST /api/users -> Crea un nuevo usuario
    @PostMapping
    public ResponseEntity<User> createUser(@RequestBody User user) {
        User savedUser = userService.saveUser(user);
        return ResponseEntity.ok(savedUser); // 200 OK con el usuario guardado
    }

    // DELETE /api/users/{id} -> Elimina un usuario
    @DeleteMapping("/{id}")
    public ResponseEntity<Void> deleteUser(@PathVariable Integer id) {
        userService.deleteUser(id);
        return ResponseEntity.noContent().build(); // 204 sin cuerpo
    }
}