package com.galaxyimpactv.dto;

import lombok.*;

@Getter @Setter
@NoArgsConstructor @AllArgsConstructor
@Builder
public class AchievementDTO {

    private String codigo;
    private String nombre;
    private String descripcion;
    private Long progresoActual;
    private Long objetivo;
    private Boolean completado;
    private Long puntosRecompensa;
    private String fechaDesbloqueo;
}
