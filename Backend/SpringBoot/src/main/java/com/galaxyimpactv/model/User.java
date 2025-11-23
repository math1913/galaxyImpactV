package com.galaxyimpactv.model;

import jakarta.persistence.*;
import lombok.*;

import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.List;

@Entity
@Table(name = "usuario") // nombre real de la tabla en MySQL
@Getter @Setter
@NoArgsConstructor @AllArgsConstructor
@Builder
public class User {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name = "idUsuario")
    private Long idUsuario;

    @Column(name = "username", nullable = false, unique = true)
    private String username;

    @Column(name = "email", nullable = false, unique = true)
    private String email;

    @Column(name = "password", nullable = false)
    private String password;

    @Column(name = "fecha_registro")
    private LocalDateTime fecha_registro;

    @Column(name = "nivelActual")
    private Integer nivelActual;

    @Column(name = "experiencia")
    private Long experiencia;

    @ElementCollection
    @CollectionTable(name = "user_puntuaciones", joinColumns = @JoinColumn(name = "user_id"))
    @Column(name = "puntos")
    private List<Integer> puntuaciones = new ArrayList<>();

    @ManyToMany
    @JoinTable(
            name = "usuario_logro",
            joinColumns = @JoinColumn(name = "FK_idUsuario"),
            inverseJoinColumns = @JoinColumn(name = "FK_idLogro")
    )
    private List<Achievement> logros;
}
