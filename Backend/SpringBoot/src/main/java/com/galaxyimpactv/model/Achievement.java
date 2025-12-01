package com.galaxyimpactv.model;

import jakarta.persistence.*;
import lombok.*;
import java.util.List;

@Entity
@Table(name = "logro")
@Getter @Setter
@NoArgsConstructor @AllArgsConstructor
@Builder
public class Achievement {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name = "idLogro")
    private Long idLogro;

    @Column(name = "codigo", length = 100)
    private String codigo;

    @Column(name = "nombre", nullable = false, length = 100)
    private String nombre;

    @Column(name = "descripcion", length = 255)
    private String descripcion;

    @Column(name = "objetivo", nullable = false)
    private Long objetivo;

    @Column(name = "puntosRecompensa")
    private Long puntosRecompensa;

    @OneToMany(mappedBy = "logro", cascade = CascadeType.ALL, orphanRemoval = true)
    private List<UsuarioLogro> usuarioLogros;
}
