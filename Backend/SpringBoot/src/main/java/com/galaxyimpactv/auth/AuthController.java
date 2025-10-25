package com.galaxyimpactv.auth;

import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;
import java.util.Map;

/**
 * Controlador encargado de manejar las peticiones de autenticación.
 * Actualmente solo expone el endpoint de login básico.
 */
@RestController
@RequestMapping("/api/auth")
public class AuthController {

    private final AuthService authService;

    public AuthController(AuthService authService) {
        this.authService = authService;
    }

    @PostMapping("/login")
    public ResponseEntity<Map<String, Object>> login(@RequestBody Map<String, String> loginRequest) {
        String username = loginRequest.get("username");
        String password = loginRequest.get("password");

        boolean success = authService.login(username, password);

        if (success) {
            return ResponseEntity.ok(Map.of(
                "status", 200,
                "message", "Login correcto",
                "user", username
            ));
        } else {
            return ResponseEntity.status(401).body(Map.of(
                "status", 401,
                "error", "Credenciales inválidas"
            ));
        }
    }
}
