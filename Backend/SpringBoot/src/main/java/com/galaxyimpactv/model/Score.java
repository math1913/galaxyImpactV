package com.galaxyimpactv.model;

import jakarta.persistence.*;
import lombok.*;

import java.time.LocalDateTime;

@Entity
@Table(name = "puntuacion")
@Getter @Setter
@NoArgsConstructor @AllArgsConstructor
@Builder
public class Score {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name = "idPuntuacion")
    private Integer idPuntuacion;

    @Column(name = "nivel")
    private Integer nivel;

    @Column(name = "fecha")
    private LocalDateTime fecha;

    @ManyToOne
    @JoinColumn(name = "FK_idUsuario", nullable = false)
    private User usuario;
}
