# Changelog

## [7.0.0](https://github.com/ProjectLanternDI3P1P2/player-backend/compare/contracts-v6.0.0...contracts-v7.0.0) (2026-11-27)


### Fixed

* **cd:** sign the image before pushing it to Harbor ([81c80cd](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/81c80cd93677476947c491ee6a289bc8be95e24e))

## [6.0.0](https://github.com/ProjectLanternDI3P1P2/player-backend/compare/contracts-v5.0.0...contracts-v6.0.0) (2026-11-27)


### Added

* **contracts:** add GetCombatantSnapshot to the hero contract ([0494321](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/0494321226fe32cefd1456481c103d983f10659b))

## [5.0.0](https://github.com/ProjectLanternDI3P1P2/player-backend/compare/contracts-v4.0.0...contracts-v5.0.0) (2026-11-25)


### Added

* **contracts:** add the hero appearance read contract ([2fa2bb0](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/2fa2bb0a8d1bed449d2397ec15e1477d1abe7f13))
* **hero:** expose the hero appearance over gRPC ([c79069d](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/c79069dc27f183248181dfcdc49a2309920019a6))

## [4.0.0](https://github.com/ProjectLanternDI3P1P2/player-backend/compare/contracts-v3.0.0...contracts-v4.0.0) (2026-10-23)


### Fixed

* **player:** merge hero appearance with session changes ([5f96911](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/5f969118ab4ad17e16e5c7af7af303d0f15ecefd))

## [3.0.0](https://github.com/ProjectLanternDI3P1P2/player-backend/compare/contracts-v2.0.0...contracts-v3.0.0) (2026-09-30)


### Added

* **player:** implement player data model persistence ([94062d8](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/94062d8b3558a5245c361ffe3b46407582f9662b))
* **player:** implement player data model persistence ([89244c7](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/89244c7cf6764fae54f99eca78a05e0be9991788))

## [2.0.0](https://github.com/ProjectLanternDI3P1P2/player-backend/compare/contracts-v1.0.0...contracts-v2.0.0) (2026-09-23)


### Added

* publish player created events through RabbitMQ ([40727b6](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/40727b6125a422e1cbbeefaab2d17520c4ce2650))
* publish player created events through RabbitMQ ([cccf22e](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/cccf22eebfad251aee42150c4ba83fea8879c20c))

## 1.0.0 (2026-09-22)


### ⚠ BREAKING CHANGES

* **contracts:** release protobuf contracts independently

### Added

* **contracts:** publish protobuf package ([5df7049](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/5df7049dcb876f7f821bc58759f0fef43d7e6ec6))
* **contracts:** release protobuf contracts independently ([d7cefcb](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/d7cefcb8b517075887b9f33c90c4262bac64a212))
* Mise en place d'un exemple de service GRPC client et serveur ([550c759](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/550c759e0f5de9d7d8cebb07b2f489016918cb07))

## Changelog

All notable changes to the Protocol Buffer contracts are documented in this file.

Contract releases use the form `Vn` (`n.0.0` in the NuGet package and the
`contracts-vn.0.0` Git tag). A release is created only when files under
`Player.Contracts/` change.
