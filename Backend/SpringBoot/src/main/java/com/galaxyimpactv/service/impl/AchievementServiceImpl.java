package com.galaxyimpactv.service.impl;

import com.galaxyimpactv.dto.AchievementBatchRequest;
import com.galaxyimpactv.dto.AchievementDTO;
import com.galaxyimpactv.model.*;
import com.galaxyimpactv.repository.*;

import com.galaxyimpactv.service.AchievementService;
import com.galaxyimpactv.service.UserService;

import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;

import java.time.LocalDateTime;
import java.util.List;
import java.util.stream.Collectors;

@Service
@RequiredArgsConstructor
public class AchievementServiceImpl implements AchievementService {

    private final UserRepository userRepository;
    private final AchievementRepository achievementRepository;
    private final UsuarioLogroRepository usuarioLogroRepository;
    private final UserService userService;

	@Override
	public void addProgress(Long userId, String codigoLogro, long amount) {

	// Buscar usuario
	User usuario = userRepository.findById(userId)  
			.orElseThrow(() -> new RuntimeException("Usuario no encontrado"));

	// Buscar logro
	Achievement logro = achievementRepository.findByCodigo(codigoLogro)
			.orElseThrow(() -> new RuntimeException("Logro no encontrado: " + codigoLogro));

	// Buscar progreso usuario-logro
	UsuarioLogro usuarioLogro = usuarioLogroRepository
			.findByUsuarioAndLogro(usuario, logro)
			.orElse(null);

	if (usuarioLogro == null) {
			usuarioLogro = UsuarioLogro.builder()
					.usuario(usuario)
					.logro(logro)
					.progresoActual(0L)
					.completado(false)
					.build();
	}

	// Sumar progreso
	long nuevoProgreso = usuarioLogro.getProgresoActual() + amount;
	usuarioLogro.setProgresoActual(nuevoProgreso);

	// COMPLETADO
	if (!usuarioLogro.getCompletado() && nuevoProgreso >= logro.getObjetivo()) {

			usuarioLogro.setCompletado(true);
			usuarioLogro.setFechaDesbloqueo(LocalDateTime.now());
			// Actualizar puntos de experiencia del usuario
			userService.updateStats(userId, 0, logro.getPuntosRecompensa());
	}

    usuarioLogroRepository.save(usuarioLogro);
    }


    @Override
    public List<AchievementDTO> getLogrosUsuario(Long userId) {

        User usuario = userRepository.findById(userId)
                .orElseThrow(() -> new RuntimeException("Usuario no encontrado"));

        List<UsuarioLogro> lista = usuarioLogroRepository.findByUsuario(usuario);

        return lista.stream()
                .map(ul -> AchievementDTO.builder()
                        .codigo(ul.getLogro().getCodigo())
                        .nombre(ul.getLogro().getNombre())
                        .descripcion(ul.getLogro().getDescripcion())
                        .progresoActual(ul.getProgresoActual())
                        .objetivo(ul.getLogro().getObjetivo())
                        .completado(ul.getCompletado())
                        .puntosRecompensa(ul.getLogro().getPuntosRecompensa())
                        .fechaDesbloqueo(
                                ul.getFechaDesbloqueo() != null ? ul.getFechaDesbloqueo().toString() : null
                        )
                        .build()
                )
                .collect(Collectors.toList());
    }
	@Override
	public void processBatch(AchievementBatchRequest r) {

		Long userId = r.getUserId();

		// 1) Kills normales
		addProgress(userId, "KILL_NORMAL_100", r.getKillsNormal());
		addProgress(userId, "KILL_NORMAL_300", r.getKillsNormal());
		addProgress(userId, "KILL_NORMAL_700", r.getKillsNormal());
		addProgress(userId, "KILL_NORMAL_1500", r.getKillsNormal());

		// 2) Kills fast
		addProgress(userId, "KILL_FAST_50", r.getKillsFast());
		addProgress(userId, "KILL_FAST_150", r.getKillsFast());
		addProgress(userId, "KILL_FAST_400", r.getKillsFast());
		addProgress(userId, "KILL_FAST_800", r.getKillsFast());

		// 3) Kills tank
		addProgress(userId, "KILL_TANK_25", r.getKillsTank());
		addProgress(userId, "KILL_TANK_75", r.getKillsTank());
		addProgress(userId, "KILL_TANK_200", r.getKillsTank());
		addProgress(userId, "KILL_TANK_400", r.getKillsTank());

		// 4) Kills shooter
		addProgress(userId, "KILL_SHOOTER_10", r.getKillsShooter());
		addProgress(userId, "KILL_SHOOTER_40", r.getKillsShooter());
		addProgress(userId, "KILL_SHOOTER_100", r.getKillsShooter());
		addProgress(userId, "KILL_SHOOTER_250", r.getKillsShooter());

		// 5) Tiempo
		addProgress(userId, "TIME_SURVIVE_30", r.getMinutesPlayed());
		addProgress(userId, "TIME_SURVIVE_60", r.getMinutesPlayed());
		addProgress(userId, "TIME_SURVIVE_180", r.getMinutesPlayed());
		addProgress(userId, "TIME_SURVIVE_360", r.getMinutesPlayed());
		addProgress(userId, "TIME_SURVIVE_720", r.getMinutesPlayed());

		// 6) Score
		addProgress(userId, "SCORE_SINGLE_200", r.getScore());
		addProgress(userId, "SCORE_SINGLE_500", r.getScore());
		addProgress(userId, "SCORE_SINGLE_800", r.getScore());
		addProgress(userId, "SCORE_SINGLE_1200", r.getScore());
		addProgress(userId, "SCORE_SINGLE_1600", r.getScore());
		addProgress(userId, "SCORE_SINGLE_2500", r.getScore());

		// 7) Pickups
		addProgress(userId, "PICKUP_HEALTH_25", r.getPickupHealth());
		addProgress(userId, "PICKUP_HEALTH_100", r.getPickupHealth());
		addProgress(userId, "PICKUP_HEALTH_250", r.getPickupHealth());

		addProgress(userId, "PICKUP_SHIELD_25", r.getPickupShield());
		addProgress(userId, "PICKUP_SHIELD_100", r.getPickupShield());
		addProgress(userId, "PICKUP_SHIELD_250", r.getPickupShield());

		addProgress(userId, "PICKUP_AMMO_50", r.getPickupAmmo());
		addProgress(userId, "PICKUP_AMMO_200", r.getPickupAmmo());
		addProgress(userId, "PICKUP_AMMO_500", r.getPickupAmmo());

		addProgress(userId, "PICKUP_EXP_50", r.getPickupExp());
		addProgress(userId, "PICKUP_EXP_200", r.getPickupExp());
		addProgress(userId, "PICKUP_EXP_500", r.getPickupExp());
	}

}
